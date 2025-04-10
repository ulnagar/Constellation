namespace Constellation.Application.Attendance.GenerateHistoricalDailyAttendanceReport;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Timetables;
using Constellation.Core.Models.Timetables.Enums;
using Constellation.Core.Models.Timetables.Repositories;
using Core.Models.Enrolments;
using Core.Models.Students.Enums;
using Core.Shared;
using Helpers;
using Interfaces.Gateways;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


internal sealed class GenerateHistoricalDailyAttendanceReportQueryHandler
: IQueryHandler<GenerateHistoricalDailyAttendanceReportQuery, FileDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ISentralGateway _gateway;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GenerateHistoricalDailyAttendanceReportQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        ISentralGateway gateway,
        IExcelService excelService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _gateway = gateway;
        _excelService = excelService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GenerateHistoricalDailyAttendanceReportQuery>();
    }

    public async Task<Result<FileDto>> Handle(GenerateHistoricalDailyAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        Result<(DateOnly StartDate, DateOnly EndDate)> startDates = await _gateway.GetDatesForWeek(request.Year, request.Start.Term, request.Start.Week);
        Result<(DateOnly StartDate, DateOnly EndDate)> endDates = await _gateway.GetDatesForWeek(request.Year, request.End.Term, request.End.Week);

        DateOnly startDate = startDates.Value.StartDate;
        DateOnly endDate = endDates.Value.EndDate;
        DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        DateTime endDateTime = startDate.ToDateTime(TimeOnly.MinValue);

        List<Student> students = await _studentRepository.GetEnrolledForDates(startDate, endDate, cancellationToken);

        _logger.Information("Found {students} students to process", students.Count);

        List<SefAttendanceData> records = [];

        foreach (Student student in students)
        {
            _logger.Information("Processing {student}", student.Name.DisplayName);

            string sentralId = student.SystemLinks.FirstOrDefault(link => link.System.Equals(SystemType.Sentral))?.Value;

            if (string.IsNullOrWhiteSpace(sentralId))
            {
                _logger.Warning("{student} has no SentralId registered!", student.Name.DisplayName);
                continue;
            }

            Result<List<DateOnly>> enrolledDates = await _gateway.GetEnrolledDatesForStudent(sentralId, request.Year, startDate, endDate);

            if (enrolledDates.IsFailure)
            {
                _logger
                .ForContext(nameof(Error), enrolledDates.Error)
                    .Warning("Could not retrieve enrolled dates for {student}", student.Name.DisplayName);
                continue;
            }
            int enrolledDays = enrolledDates.Value.Count;

            if (enrolledDays == 0)
            {
                _logger.Warning("{student} was not enrolled during period", student.Name.DisplayName);

                continue;
            }

            Dictionary<DateOnly, string> absentDates = new();
            Dictionary<DateOnly, string> justifiedAbsentDates = new();

            // Get active student enrolments for the period
            List<Enrolment> enrolments = await _enrolmentRepository.GetHistoricalForStudent(student.Id, startDate, endDate, cancellationToken);

            // Get offerings from enrolments
            List<Offering> offerings = await _offeringRepository.GetListFromIds(enrolments.Select(enrolment => enrolment.OfferingId).ToList(), cancellationToken);

            offerings = offerings
            .Where(offering => offering.Name.Value[2] != 'T')
                .ToList();

            if (offerings.Count == 0)
            {
                _logger.Warning("{student} had no valid enrolled classes during period", student.Name.DisplayName);

                continue;
            }

            // Get sessions from offerings
            List<Session> sessions = offerings
                .SelectMany(offering => offering.Sessions)
                .Where(session =>
                    session.CreatedAt < endDateTime &&
                    (session.DeletedAt == DateTime.MinValue || session.DeletedAt > startDateTime))
                .ToList();

            // Get periods from sessions
            List<Period> periods = await _periodRepository.GetListFromIds(
                sessions.Select(session => session.PeriodId)
                    .Distinct()
                    .ToList(),
                cancellationToken);

            periods = periods
                .Where(period => period.Duration > 0)
                .ToList();

            // Calculate # classes per day (Stage 3 always 1, Stage 4 & 5 either 1 or 2, Stage 6 max 3)
            Dictionary<DayOfWeek, int> numOfferingsPerDay = new();

            foreach (PeriodWeek week in PeriodWeek.GetOptions)
            {
                foreach (PeriodDay day in PeriodDay.GetOptions)
                {
                    IEnumerable<Period> dayPeriods = periods.Where(period => period.Week.Equals(week) && period.Day.Equals(day));
                    int dayOfferings = sessions.Where(session =>
                            dayPeriods.Select(period => period.Id).Contains(session.PeriodId))
                        .Select(session => session.OfferingId)
                        .Distinct()
                        .Count();

                    DayOfWeek dayOfWeek = day switch
                    {
                        _ when day.Equals(PeriodDay.Monday) => DayOfWeek.Monday,
                        _ when day.Equals(PeriodDay.Tuesday) => DayOfWeek.Tuesday,
                        _ when day.Equals(PeriodDay.Wednesday) => DayOfWeek.Wednesday,
                        _ when day.Equals(PeriodDay.Thursday) => DayOfWeek.Thursday,
                        _ when day.Equals(PeriodDay.Friday) => DayOfWeek.Friday
                    };

                    bool success = numOfferingsPerDay.TryGetValue(dayOfWeek, out int existingEntry);

                    if (!success)
                    {
                        numOfferingsPerDay.TryAdd(dayOfWeek, dayOfferings);
                        continue;
                    }

                    if (existingEntry < dayOfferings)
                    {
                        numOfferingsPerDay[dayOfWeek] = dayOfferings;
                    }
                }
            }

            // Get whole absences for the student for the period
            List<Absence> wholeAbsences = await _absenceRepository.GetStudentWholeAbsencesForDateRange(student.Id, startDate, endDate, cancellationToken);

            _logger.Information("{student} had {absences} whole absences during period", student.Name.DisplayName, wholeAbsences.Count);
            List<SentralPeriodAbsenceDto> sentralAbsences = [];

            // Only get Sentral Attendance absences for the student if there are any absences to confirm
            if (wholeAbsences.Count > 0)
            {
                Result<List<SentralPeriodAbsenceDto>> sentralAbsencesRequest = await _gateway.GetAbsenceDataAsync(sentralId, request.Year, cancellationToken);
                if (sentralAbsencesRequest.IsFailure)
                {
                    _logger
                        .Warning("Failed to retrieve Sentral Absence data for {student}", student.Name.DisplayName);
                    
                    continue;
                }

                sentralAbsences = sentralAbsencesRequest.Value
                    .Where(entry => enrolledDates.Value.Contains(entry.Date))
                    .ToList();
            }

            var groupedAbsences = wholeAbsences.GroupBy(absence => absence.Date);

            // Match absences with days to determine whether it was whole day or part day
            foreach (var group in groupedAbsences)
            {
                DayOfWeek day = group.Key.DayOfWeek;
                if (group.Count() != numOfferingsPerDay[day])
                {
                    continue;
                }

                // Calculate whether the absences were School Business/Shared Enrollment
                // If it is, add to separate list to duplicate percentage for absences only
                foreach (Absence absence in group)
                {
                    SentralPeriodAbsenceDto matchingSentralAbsence = sentralAbsences.FirstOrDefault(entry => entry.Date == absence.Date);

                    if (matchingSentralAbsence is null)
                    {
                        absence.UpdateAbsenceReason(AbsenceReason.SharedEnrolment);
                        continue;
                    }

                    if (absence.AbsenceReason != matchingSentralAbsence.Reason)
                    {
                        _logger
                            .ForContext(nameof(Absence), absence, true)
                            .Information("Updating absence ({date}) reason from {oldReason} to {newReason}",
                                absence.Date, absence.AbsenceReason, matchingSentralAbsence.Reason);

                        absence.UpdateAbsenceReason((AbsenceReason)TypeDescriptor.GetConverter(typeof(AbsenceReason)).ConvertFromString(matchingSentralAbsence.Reason));
                    }
                }

                if (group.All(absence => absence.AbsenceReason == AbsenceReason.SchoolBusiness || absence.AbsenceReason == AbsenceReason.SharedEnrolment))
                {
                    justifiedAbsentDates.Add(group.Key, string.Join(",", group.Select(entry => entry.AbsenceReason.Name)));
                }

                absentDates.Add(group.Key, string.Join(",", group.Select(entry => entry.AbsenceReason?.Name)));
            }

            Grade grade = offerings.First().Name.Value[..2] switch
            {
                "05" => Grade.Y05,
                "06" => Grade.Y06,
                "07" => Grade.Y07,
                "08" => Grade.Y08,
                "09" => Grade.Y09,
                "10" => Grade.Y10,
                "11" => Grade.Y11,
                "12" => Grade.Y12,
                _ => Grade.SpecialProgram
            };

            records.Add(new(
                sentralId,
                student.StudentReferenceNumber,
                student.Name,
                grade,
                offerings.Select(entry => entry.Name.Value).ToList(),
                enrolledDates.Value.Count,
                absentDates.Count,
                enrolledDates.Value.Count - absentDates.Count,
                (enrolledDates.Value.Count - absentDates.Count) / (decimal)enrolledDates.Value.Count,
                justifiedAbsentDates.Count,
                absentDates.Count - justifiedAbsentDates.Count,
                enrolledDates.Value.Count - (absentDates.Count - justifiedAbsentDates.Count),
                (enrolledDates.Value.Count - (absentDates.Count - justifiedAbsentDates.Count)) / (decimal)enrolledDates.Value.Count,
                absentDates.Select(entry => $"{entry.Key.ToString("dd/MM/yyyy")} - {entry.Value}").ToList(),
                justifiedAbsentDates.Select(entry => $"{entry.Key.ToString("dd/MM/yyyy")} - {entry.Value}").ToList()));
        }

        _logger.Information("Building export file");
        MemoryStream report = await _excelService.CreateSefAttendanceDataExport(records, cancellationToken);

        FileDto file = new()
        {
            FileData = report.ToArray(), 
            FileName = "Historical Attendance Report.xlsx", 
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}