namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Repositories;
using DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public class GenerateAttendanceReportForStudentQueryHandler 
    : IQueryHandler<GenerateAttendanceReportForStudentQuery, FileDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IExportService _exportService;
    private readonly ISentralGateway _sentralGateway;

    public GenerateAttendanceReportForStudentQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IPeriodRepository periodRepository,
        IExportService exportService,
        ISentralGateway sentralGateway)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _periodRepository = periodRepository;
        _exportService = exportService;
        _sentralGateway = sentralGateway;
    }

    public async Task<Result<FileDto>> Handle(GenerateAttendanceReportForStudentQuery request, CancellationToken cancellationToken)
    {
        List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(request.StartDate.Year.ToString());

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        DateOnly startDate = request.StartDate.VerifyStartOfFortnight();
        DateOnly endDate = startDate.AddDays(12);

        List<Absence> absences = await _absenceRepository.GetForStudentFromDateRange(student.Id, startDate, endDate, cancellationToken);

        List<AttendanceAbsenceDetail> absenceDetails = new();

        foreach (Absence absence in absences) 
        {
            Response response = absence.GetExplainedResponse();

            string explanation = string.Empty;

            if (response is null)
                explanation = "<br />No explanation provided yet.";

            if (response is not null && response.VerificationStatus != ResponseVerificationStatus.Rejected)
            {
                explanation = $"<br />Explained by {response.Type.Value}";

                if (response.Type != ResponseType.System)
                {
                    explanation += $" ({response.From})";
                }

                explanation += $": {response.Explanation}";

                if (response.Type == ResponseType.Student)
                {
                    if (response.VerificationStatus == ResponseVerificationStatus.Pending)
                        explanation += $"<br /> - Pending verification from school";

                    if (response.VerificationStatus == ResponseVerificationStatus.Verified)
                        explanation += $"<br /> - Verified by {response.Verifier}";
                }
            }

            AttendanceAbsenceDetail entry = new(
                absence.Date,
                absence.OfferingId,
                absence.StartTime,
                absence.Type,
                absence.AbsenceTimeframe,
                absence.AbsenceReason,
                explanation,
                absence.Explained);

            absenceDetails.Add(entry);
        }

        List<DateOnly> reportableDates = startDate.Range(endDate);
        List<AttendanceDateDetail> attendanceDates = new();

        foreach (DateOnly date in reportableDates)
        {
            if (excludedDates.Contains(date) || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            PeriodWeek week = PeriodWeek.FromDayNumber(date.GetDayNumber());
            PeriodDay day = PeriodDay.FromDayNumber(date.GetDayNumber());

            List<Offering> offerings = await _offeringRepository.GetCurrentEnrolmentsFromStudentForDate(student.Id, date, week, day, cancellationToken);
            List<AttendanceDateDetail.SessionWithOffering> sessionDetails = new();

            foreach (Offering offering in offerings)
            {
                Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                List<Period> periods = await _periodRepository.GetForOfferingOnDay(offering.Id, date, week, day, cancellationToken);
                Period firstPeriod = periods.First(period => period.StartTime == periods.Min(p => p.StartTime));
                Period lastPeriod = periods.First(period => period.EndTime == periods.Max(p => p.EndTime));

                if (periods.Count() == 1)
                {
                    sessionDetails.Add(new(
                        periods.First().Name,
                        $"{firstPeriod.StartTime.As12HourTime()} - {lastPeriod.EndTime.As12HourTime()}",
                        offering.Name,
                        course.Name,
                        offering.Id));
                }
                else
                {
                    sessionDetails.Add(new(
                        $"{firstPeriod.Name} - {lastPeriod.Name}",
                        $"{firstPeriod.StartTime.As12HourTime()} - {lastPeriod.EndTime.As12HourTime()}",
                        offering.Name,
                        course.Name,
                        offering.Id));
                }
            }

            AttendanceDateDetail entry = new(
                date,
                date.GetDayNumber(),
                sessionDetails);

            attendanceDates.Add(entry);
        }

        MemoryStream fileStream = await _exportService.CreateAttendanceReport(
            student,
            startDate,
            excludedDates,
            absenceDetails,
            attendanceDates);

        var fileName = $"{student.Name.LastName}, {student.Name.FirstName} - {request.StartDate:yyyy-MM-dd} - Attendance Report.pdf";

        var result = new FileDto
        {
            FileName = fileName,
            FileData = fileStream.ToArray(),
            FileType = MediaTypeNames.Application.Pdf
        };

        return result;
    }
}
