namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public class GenerateAttendanceReportForStudentQueryHandler 
    : IQueryHandler<GenerateAttendanceReportForStudentQuery, StoredFile>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IExportService _exportService;
    private readonly ISentralGateway _sentralGateway;

    public GenerateAttendanceReportForStudentQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        ITimetablePeriodRepository periodRepository,
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

    public async Task<Result<StoredFile>> Handle(GenerateAttendanceReportForStudentQuery request, CancellationToken cancellationToken)
    {
        List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(request.StartDate.Year.ToString());

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        DateOnly startDate = request.StartDate.VerifyStartOfFortnight();
        DateOnly endDate = startDate.AddDays(12);

        List<Absence> absences = await _absenceRepository.GetForStudentFromDateRange(student.StudentId, startDate, endDate, cancellationToken);

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
            List<Offering> offerings = await _offeringRepository.GetCurrentEnrolmentsFromStudentForDate(student.StudentId, date, date.GetDayNumber(), cancellationToken);
            List<AttendanceDateDetail.SessionWithOffering> sessionDetails = new();

            foreach (Offering offering in offerings)
            {
                Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                List<TimetablePeriod> periods = await _periodRepository.GetForOfferingOnDay(offering.Id, date, date.GetDayNumber(), cancellationToken);
                TimetablePeriod firstPeriod = periods.First(period => period.StartTime == periods.Min(p => p.StartTime));
                TimetablePeriod lastPeriod = periods.First(period => period.EndTime == periods.Max(p => p.EndTime));

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

        var fileName = $"{student.LastName}, {student.FirstName} - {request.StartDate:yyyy-MM-dd} - Attendance Report.pdf";

        var result = new StoredFile
        {
            Name = fileName,
            FileData = fileStream.ToArray(),
            FileType = MediaTypeNames.Application.Pdf
        };

        return result;
    }
}
