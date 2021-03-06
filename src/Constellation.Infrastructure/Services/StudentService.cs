using Constellation.Application.DTOs;
using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class StudentService : IStudentService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        private IEnrolmentService _enrolService { get; set; }
        private IDeviceService _deviceService { get; set; }

        public StudentService(IUnitOfWork unitOfWork, IEnrolmentService enrolmentService,
            IDeviceService deviceService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _enrolService = enrolmentService;
            _deviceService = deviceService;
            _mediator = mediator;
        }

        public async Task<ServiceOperationResult<Student>> CreateStudent(StudentDto studentResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Student>();

            if (await _unitOfWork.Students.AnyWithId(studentResource.StudentId))
            {
                result.Success = false;
                result.Errors.Add($"A student with that ID already exists!");

                return result;
            }

            var student = new Student()
            {
                StudentId = studentResource.StudentId,
                FirstName = studentResource.FirstName,
                LastName = studentResource.LastName,
                PortalUsername = studentResource.PortalUsername,
                AdobeConnectPrincipalId = studentResource.AdobeConnectPrincipalId,
                CurrentGrade = studentResource.CurrentGrade,
                EnrolledGrade = studentResource.EnrolledGrade,
                Gender = studentResource.Gender,
                SchoolCode = studentResource.SchoolCode
            };

            _unitOfWork.Add(student);

            result.Success = true;
            result.Entity = student;

            return result;
        }

        public async Task EnrolStudentInClass(string studentId, int offeringId)
        {
            if (!await _unitOfWork.Students.AnyWithId(studentId))
                return;

            await _enrolService.CreateEnrolment(studentId, offeringId, DateTime.Now);
        }

        public async Task ReinstateStudent(string studentId)
        {
            // Validate entries
            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            if (student == null || student.DateDeleted == null)
                return;

            // Calculate new grade
            var yearLeft = student.DateDeleted.Value.Year;
            var previousGrade = (int)student.CurrentGrade;
            var thisYear = DateTime.Now.Year;
            var difference = thisYear - yearLeft;
            var thisGrade = previousGrade + difference;
            if (thisGrade > 12 || thisGrade == previousGrade)
            {
                // Do NOTHING!
            }
            else if (Enum.IsDefined(typeof(Grade), thisGrade))
            {
                var newGrade = (Grade)thisGrade;
                student.CurrentGrade = newGrade;
            }

            student.IsDeleted = false;
            student.DateDeleted = null;
        }

        public async Task RemoveStudent(string studentId)
        {
            // Validate entries
            var student = await _unitOfWork.Students.ForDeletion(studentId);

            if (student == null)
                return;

            // Remove all current student enrolments
            foreach (var enrol in student.Enrolments.Where(e => !e.IsDeleted))
            {
                await UnenrolStudentFromClass(student.StudentId, enrol.OfferingId);
            }

            // Unassign devices
            foreach (var device in student.Devices.Where(d => d.IsDeleted == false))
            {
                await _deviceService.DeallocateDevice(device.SerialNumber);
            }

            student.IsDeleted = true;
            student.DateDeleted = DateTime.Now;
        }

        public async Task UnenrolStudentFromClass(string studentId, int offeringId)
        {
            // Validate entries
            var student = await _unitOfWork.Students.ForBulkUnenrolAsync(studentId);

            if (student == null)
                return;

            foreach (var enrolment in student.Enrolments.Where(e => e.OfferingId == offeringId && !e.IsDeleted))
            {
                await _enrolService.RemoveEnrolment(enrolment.Id);
            }
        }

        public async Task<ServiceOperationResult<Student>> UpdateStudent(string studentId, StudentDto studentResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Student>();

            // Validate entries
            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            if (student == null)
            {
                result.Success = false;
                result.Errors.Add($"A student with that ID could not be found!");

                return result;
            }

            // Update properties
            if (!string.IsNullOrWhiteSpace(studentResource.FirstName))
                student.FirstName = studentResource.FirstName;

            if (!string.IsNullOrWhiteSpace(studentResource.LastName))
                student.LastName = studentResource.LastName;

            if (!string.IsNullOrWhiteSpace(studentResource.PortalUsername))
                student.PortalUsername = studentResource.PortalUsername;

            if (!string.IsNullOrWhiteSpace(studentResource.AdobeConnectPrincipalId))
                student.AdobeConnectPrincipalId = studentResource.AdobeConnectPrincipalId;

            if (!string.IsNullOrWhiteSpace(studentResource.SentralStudentId))
                student.SentralStudentId = studentResource.SentralStudentId;

            if (studentResource.CurrentGrade != 0)
                student.CurrentGrade = studentResource.CurrentGrade;

            if (studentResource.EnrolledGrade != 0)
                student.EnrolledGrade = studentResource.EnrolledGrade;

            if (!string.IsNullOrWhiteSpace(studentResource.Gender))
                student.Gender = studentResource.Gender;

            if (!string.IsNullOrWhiteSpace(studentResource.SchoolCode))
            {
                if (student.SchoolCode != studentResource.SchoolCode)
                {
                    await _mediator.Publish(new StudentMovedSchoolsNotification { StudentId = student.StudentId, OldSchoolCode = student.SchoolCode, NewSchoolCode = studentResource.SchoolCode });
                    student.SchoolCode = studentResource.SchoolCode;
                }
            }

            result.Success = true;
            result.Entity = student;

            return result;
        }

        public async Task EnableStudentAbsenceNotifications(string studentId, DateTime startDate)
        {
            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            if (student == null)
                return;

            await student.EnableAbsenceNotifications(startDate);
        }
    }
}
