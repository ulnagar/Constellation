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

        public ExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<InterviewExportDto> CreatePTOExport(ICollection<Student> students, bool perFamily)
        {
            var result = new List<InterviewExportDto>();

            foreach (var student in students)
            {
                var validEnrolments = student.Enrolments.Where(enrol => !enrol.IsDeleted && enrol.Offering.IsCurrent()).ToList();
                 
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

                    if (perFamily)
                    {
                        var lastName = student.Family.Address.Title.Split(' ').Last();
                        var firstName = student.Family.Address.Title.Substring(0, student.Family.Address.Title.Length - lastName.Length).Trim();
                        var email = student.Family.Parent1.EmailAddress ?? student.Family.Parent2.EmailAddress;

                        var entry = new InterviewExportDto.Parent
                        {
                            ParentCode = email,
                            ParentFirstName = firstName,
                            ParentLastName = lastName,
                            ParentEmailAddress = email
                        };

                        dto.Parents.Add(entry);

                        result.Add(dto);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(student.Family.Parent1.FirstName) && !string.IsNullOrWhiteSpace(student.Family.Parent2.FirstName) && student.Family.Parent1.EmailAddress == student.Family.Parent2.EmailAddress)
                    {
                        // Create single family login

                        var lastName = student.Family.Address.Title.Split(' ').Last();
                        var firstName = student.Family.Address.Title.Substring(0, student.Family.Address.Title.Length - lastName.Length).Trim();

                        var entry = new InterviewExportDto.Parent
                        {
                            ParentCode = student.Family.Parent1.EmailAddress,
                            ParentFirstName = firstName,
                            ParentLastName = lastName,
                            ParentEmailAddress = student.Family.Parent1.EmailAddress
                        };

                        dto.Parents.Add(entry);

                        result.Add(dto);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(student.Family.Parent1.FirstName))
                    {
                        var entry = new InterviewExportDto.Parent
                        {
                            ParentCode = student.Family.Parent1.EmailAddress,
                            ParentFirstName = student.Family.Parent1.FirstName,
                            ParentLastName = student.Family.Parent1.LastName,
                            ParentEmailAddress = student.Family.Parent1.EmailAddress
                        };

                        dto.Parents.Add(entry);
                    }

                    if (!string.IsNullOrWhiteSpace(student.Family.Parent2.FirstName))
                    {
                        var entry = new InterviewExportDto.Parent
                        {
                            ParentCode = student.Family.Parent2.EmailAddress,
                            ParentFirstName = student.Family.Parent2.FirstName,
                            ParentLastName = student.Family.Parent2.LastName,
                            ParentEmailAddress = student.Family.Parent2.EmailAddress
                        };

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
