using Constellation.Application.DTOs;
using Constellation.Application.Enrolments.EnrolStudent;
using Constellation.Application.Enrolments.UnenrolStudent;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    using Application.Students.UpdateStudent;

    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class StudentsController : Controller
    {
        private readonly IOfferingRepository _offeringRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public StudentsController(
            IOfferingRepository offeringRepository,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _offeringRepository = offeringRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Active");
        }

        public async Task<IActionResult> All()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => true);

            var viewModel = new Student_ViewModel();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Active()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => !student.IsDeleted);

            foreach (var student in students)
            {
                if (student.AbsenceConfigurations.Count == 0 && student.IncludeInAbsenceNotifications)
                {
                    Result<AbsenceConfiguration> wholeRequest = AbsenceConfiguration.Create(student.StudentId, AbsenceType.Whole, DateOnly.FromDateTime(student.AbsenceNotificationStartDate.Value), null);

                    if (wholeRequest.IsSuccess)
                        student.AddAbsenceConfiguration(wholeRequest.Value);

                    Result<AbsenceConfiguration> partialRequest = AbsenceConfiguration.Create(student.StudentId, AbsenceType.Partial, DateOnly.FromDateTime(student.AbsenceNotificationStartDate.Value), null);

                    if (partialRequest.IsSuccess)
                        student.AddAbsenceConfiguration(partialRequest.Value);

                    await _unitOfWork.CompleteAsync();
                }
            }

            var viewModel = new Student_ViewModel();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Inactive()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => student.IsDeleted);

            var viewModel = new Student_ViewModel();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutACDetails()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => string.IsNullOrWhiteSpace(student.AdobeConnectPrincipalId));

            var viewModel = new Student_ViewModel();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutDevice()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => student.Devices.Any(device => !device.IsDeleted));

            var viewModel = new Student_ViewModel();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var student = await _unitOfWork.Students.ForEditAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var genders = new List<SelectListItem>()
            {
                new SelectListItem() { Text ="Male", Value = "M" },
                new SelectListItem() { Text = "Female", Value = "F" }
            };

            var viewModel = new Student_UpdateViewModel();
            viewModel.Student = new StudentDto
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                PortalUsername = student.PortalUsername,
                AdobeConnectPrincipalId = student.AdobeConnectPrincipalId,
                SentralStudentId = student.SentralStudentId,
                EnrolledGrade = student.EnrolledGrade,
                CurrentGrade = student.CurrentGrade,
                Gender = student.Gender,
                SchoolCode = student.SchoolCode
            };
            viewModel.IsNew = false;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name", student.SchoolCode);
            viewModel.GenderList = new SelectList(genders, "Value", "Text", student.Gender);

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Student_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var schools = await _unitOfWork.Schools.ForSelectionAsync();
                var genders = new List<SelectListItem>()
                {
                    new SelectListItem() { Text ="Male", Value = "M" },
                    new SelectListItem() { Text = "Female", Value = "F" }
                };

                viewModel.SchoolList = new SelectList(schools, "Code", "Name", viewModel.Student.SchoolCode);
                viewModel.GenderList = new SelectList(genders, "Value", "Text", viewModel.Student.Gender);

                return View("Update", viewModel);
            }

            if (viewModel.IsNew)
            {
                Student student = Student.Create(
                    viewModel.Student.StudentId,
                    viewModel.Student.FirstName,
                    viewModel.Student.LastName,
                    viewModel.Student.PortalUsername,
                    viewModel.Student.CurrentGrade,
                    viewModel.Student.SchoolCode,
                    viewModel.Student.Gender);

                _unitOfWork.Students.Insert(student);
            }
            else
            {
                await _mediator.Send(new UpdateStudentCommand(
                    viewModel.Student.StudentId,
                    viewModel.Student.FirstName,
                    viewModel.Student.LastName,
                    viewModel.Student.PortalUsername,
                    viewModel.Student.AdobeConnectPrincipalId,
                    viewModel.Student.SentralStudentId,
                    viewModel.Student.CurrentGrade,
                    viewModel.Student.EnrolledGrade,
                    viewModel.Student.Gender,
                    viewModel.Student.SchoolCode));
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var genders = new List<SelectListItem>()
            {
                new SelectListItem() { Text ="Male", Value = "M" },
                new SelectListItem() { Text = "Female", Value = "F" }
            };

            var viewModel = new Student_UpdateViewModel();
            viewModel.IsNew = true;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name");
            viewModel.GenderList = new SelectList(genders, "Value", "Text");

            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> UnenrolAll(string id)
        {
            var student = await _unitOfWork.Students.ForBulkUnenrolAsync(id);

            foreach (var enrolment in student.Enrolments.Where(e => !e.IsDeleted))
            {
                await _mediator.Send(new UnenrolStudentCommand(student.StudentId, enrolment.OfferingId));
            }

            return RedirectToPage("/Students/Details", new { area = "Partner", id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Unenrol(string id, Guid classId)
        {
            OfferingId offeringId = OfferingId.FromValue(classId);

            if (offeringId is null)
                return RedirectToPage("/Students/Details", new { area = "Partner", id });

            await _mediator.Send(new UnenrolStudentCommand(id, offeringId));

            return RedirectToPage("/Students/Details", new { area = "Partner", id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Enrol(string id, Guid classId)
        {
            var student = await _unitOfWork.Students.ForEditAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            OfferingId offeringId = OfferingId.FromValue(classId);

            var offering = await _offeringRepository.GetById(offeringId);

            if (offering == null)
            {
                return RedirectToAction("Index");
            }

            if (!student.Enrolments.Any(e => e.OfferingId == offeringId && !e.IsDeleted))
            {
                await _mediator.Send(new EnrolStudentCommand(student.StudentId, offering.Id));
            }

            return RedirectToPage("/Students/Details", new { area = "Partner", id });
        }
    }
}