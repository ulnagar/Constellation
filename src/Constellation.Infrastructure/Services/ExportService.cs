using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class ExportService : IExportService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISentralService _sentralService;
        private readonly ISentralGateway _sentralGateway;

        public ExportService(IUnitOfWork unitOfWork, ISentralService sentralService,
            ISentralGateway sentralGateway)
        {
            _unitOfWork = unitOfWork;
            _sentralService = sentralService;
            _sentralGateway = sentralGateway;
        }

        public async Task<List<InterviewExportDto>> CreatePTOExport(ICollection<Student> students)
        {
            var result = new List<InterviewExportDto>();

            foreach (var student in students)
            {
                var validEnrolments = student.Enrolments.Where(enrol => !enrol.IsDeleted && enrol.Offering.IsCurrent()).ToList();

                var sentralId = await _sentralService.UpdateSentralStudentId(student.StudentId);

                var parents = await _sentralGateway.GetParentContactEntry(sentralId);

                foreach (var enrolment in validEnrolments)
                {
                    var course = enrolment.Offering.Course;

                    var sessions = enrolment.Offering.Sessions.Where(session => !session.IsDeleted).ToList();
                    var teachers = sessions.GroupBy(session => session.StaffId).OrderByDescending(group => group.Count());

                    var teacher = teachers.First().First().Teacher;

                    var dto = new InterviewExportDto
                    {
                        StudentId = student.StudentId,
                        StudentFirstName = student.FirstName,
                        StudentLastName = student.LastName,
                        ClassCode = enrolment.Offering.Name,
                        ClassGrade = course.Grade.AsNumber(),
                        ClassName = course.Name,
                        TeacherCode = teacher.StaffId,
                        TeacherTitle = "",
                        TeacherFirstName = teacher.FirstName,
                        TeacherLastName = teacher.LastName,
                        TeacherEmailAddress = teacher.EmailAddress
                    };

                    foreach (var parent in parents)
                    {
                        var entry = new InterviewExportDto.Parent();
                        entry.ParentCode = parent.Key;

                        foreach (var detail in parent.Value)
                        {
                            switch (detail.Key)
                            {
                                case "FirstName":
                                    entry.ParentFirstName = detail.Value;
                                    break;
                                case "LastName":
                                    entry.ParentLastName = detail.Value;
                                    break;
                                case "EmailAddress":
                                    entry.ParentEmailAddress = detail.Value;
                                    break;
                            }
                        }

                        dto.Parents.Add(entry);
                    }

                    result.Add(dto);
                }
            }

            return result;
        }

        public async Task<List<AbsenceExportDto>> CreateAbsenceExport(AbsenceFilterDto filter)
        {
            var absences = await _unitOfWork.Absences.ForReportAsync(filter);

            absences = absences.OrderBy(a => a.Date.Date)
                .ThenBy(a => a.PeriodTimeframe)
                .ThenBy(a => a.Student.School.Name)
                .ThenBy(a => a.Student.CurrentGrade)
                .ToList()
                .Where(a => a.Date.Year == DateTime.Now.Year)
                .ToList();

            var data = absences.Select(AbsenceExportDto.ConvertFromAbsence).ToList();

            return data;
        }
    }
}
