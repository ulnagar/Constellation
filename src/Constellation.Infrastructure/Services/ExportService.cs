﻿namespace Constellation.Infrastructure.Services;

using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Templates.Views.Documents.Attendance;
using Core.Extensions;
using Core.Models.Enrolments.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ExportService : IExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IRazorViewToStringRenderer _renderService;
    private readonly IPDFService _pdfService;

    public ExportService(
        IUnitOfWork unitOfWork,
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository,
        ICourseRepository courseRepository,
        IStaffRepository staffRepository,
        IFamilyRepository familyRepository,
        IRazorViewToStringRenderer renderService,
        IPDFService pdfService)
    {
        _unitOfWork = unitOfWork;
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _courseRepository = courseRepository;
        _staffRepository = staffRepository;
        _familyRepository = familyRepository;
        _renderService = renderService;
        _pdfService = pdfService;
    }

    public async Task<List<InterviewExportDto>> CreatePTOExport(
        List<Student> students,
        bool perFamily,
        bool residentialFamilyOnly,
        CancellationToken cancellationToken = default)
    {
        var result = new List<InterviewExportDto>();

        foreach (var student in students)
        {
            var families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);

            if (residentialFamilyOnly)
                families = families
                    .Where(entry =>
                        entry.Students.Any(member =>
                            member.StudentId == student.StudentId &&
                            member.IsResidentialFamily))
                    .ToList();

            var validEnrolments = await _enrolmentRepository.GetCurrentByStudentId(student.StudentId, cancellationToken);

            foreach (var family in families)
            {
                foreach (var enrolment in validEnrolments)
                {
                    var course = await _courseRepository.GetByOfferingId(enrolment.OfferingId, cancellationToken);

                    var offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

                    var teachers = await _staffRepository.GetPrimaryTeachersForOffering(enrolment.OfferingId, cancellationToken);

                    foreach (var teacher in teachers)
                    {
                        var dto = new InterviewExportDto
                        {
                            StudentId = student.StudentId,
                            StudentFirstName = student.FirstName,
                            StudentLastName = student.LastName,
                            ClassCode = offering.Name,
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
                            var lastName = family.FamilyTitle.Split(' ').Last();
                            var firstName = family.FamilyTitle[..^lastName.Length].Trim();
                            var email = family.FamilyEmail;

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

                        foreach (var parent in family.Parents)
                        {
                            var entry = new InterviewExportDto.Parent
                            {
                                ParentCode = parent.EmailAddress,
                                ParentFirstName = parent.FirstName,
                                ParentLastName = parent.LastName,
                                ParentEmailAddress = parent.EmailAddress
                            };

                            dto.Parents.Add(entry);
                        }

                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }

    public async Task<MemoryStream> CreateAttendanceReport(
        Student student,
        DateOnly startDate,
        List<DateOnly> excludedDates,
        List<AttendanceAbsenceDetail> absences,
        List<AttendanceDateDetail> dates,
        CancellationToken cancellationToken = default)
    {
        var endDate = startDate.AddDays(12);

        var viewModel = new AttendanceReportViewModel
        {
            StudentName = student.DisplayName,
            StartDate = startDate,
            ExcludedDates = excludedDates,
            Absences = absences,
            DateData = dates
        };

        var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", viewModel);
        var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", viewModel);

        return _pdfService.StringToPdfStream(htmlString, headerString);
    }
}
