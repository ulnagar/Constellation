using Constellation.Core.Models.Students.Identifiers;

namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs;
using Application.Interfaces.Services;
using BaseModels;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Absences;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Timetables;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Repositories;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ISentralGateway _gateway;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        ISentralGateway gateway,
        IExcelService excelService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _gateway = gateway;
        _excelService = excelService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        var timer = new Stopwatch();
        timer.Start();

        DateOnly startDate = new(2023, 01, 01);
        DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        DateOnly endDate = new(2023, 7, 1);
        DateTime endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

        List<Student> students = await _studentRepository.GetEnrolledForDates(startDate, endDate, default);

        List<SefAttendanceData> records = [];

        foreach (Student student in students)
        {
            string sentralId = student.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral)?.Value;

            if (string.IsNullOrWhiteSpace(sentralId))
            {
                continue;
            }

            Result<List<DateOnly>> enrolledDates = await _gateway.GetEnrolledDatesForStudent(sentralId, startDate.Year.ToString(), startDate, endDate);

            if (enrolledDates.IsFailure)
                continue;

            int enrolledDays = enrolledDates.Value.Count;

            if (enrolledDays == 0)
                continue;

            Dictionary<DateOnly, string> absentDates = new();
            Dictionary<DateOnly, string> justifiedAbsentDates = new();

            // Get active student enrolments for the period
            List<Enrolment> enrolments = await _enrolmentRepository.GetHistoricalForStudent(student.Id, startDate, endDate, default);

            // Get offerings from enrolments
            List<Offering> offerings = await _offeringRepository.GetListFromIds(enrolments.Select(enrolment => enrolment.OfferingId).ToList(), default);

            offerings = offerings
                .Where(offering => offering.Name.Value[2] != 'T')
                .ToList();

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
                default);

            periods = periods
                .Where(period => period.Duration > 0)
                .ToList();

            // Calculate # classes per day (Stage 3 always 1, Stage 4 & 5 either 1 or 2, Stage 6 max 3)
            Dictionary<DayOfWeek, int> numPeriodsPerDay = new();

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

                    bool success = numPeriodsPerDay.TryGetValue(dayOfWeek, out int existingEntry);

                    if (!success)
                    {
                        numPeriodsPerDay.TryAdd(dayOfWeek, dayOfferings);
                        continue;
                    }

                    if (existingEntry != dayOfferings)
                    {
                        // Don't know what to do about this
                        continue;
                    }
                }
            }

            // Get Sentral Attendance absences for the student
            Result<List<SentralPeriodAbsenceDto>> sentralAbsencesRequest = await _gateway.GetAbsenceDataAsync(sentralId, startDate.Year.ToString(), default);
            if (sentralAbsencesRequest.IsFailure)
            {
                //TODO: R1.17.1: Log and handle this situation properly
                continue;
            }

            List<SentralPeriodAbsenceDto> sentralAbsences = sentralAbsencesRequest.Value
                .Where(entry => enrolledDates.Value.Contains(entry.Date))
                .ToList();

            // Get whole absences for the student for the period
            List<Absence> wholeAbsences = await _absenceRepository.GetStudentWholeAbsencesForDateRange(student.Id, startDate, endDate, default);

            var groupedAbsences = wholeAbsences.GroupBy(absence => absence.Date);

            // Match absences with days to determine whether it was whole day or part day
            foreach (var group in groupedAbsences)
            {
                var day = group.Key.DayOfWeek;
                if (group.Count() != numPeriodsPerDay[day])
                {
                    continue;
                }

                // Calculate whether the absences were School Business/Shared Enrollment
                // If it is, add to separate list to duplicate percentage for absences only
                foreach (var absence in group)
                {
                    var matchingSentralAbsence = sentralAbsences.FirstOrDefault(entry => entry.Date == absence.Date);

                    if (matchingSentralAbsence is null)
                    {
                        //TODO: R1.17.1: Should this be ignored, or should the absence be discarded?
                        continue;
                    }

                    if (absence.AbsenceReason != matchingSentralAbsence.Reason)
                    {
                        absence.UpdateAbsenceReason((AbsenceReason)TypeDescriptor.GetConverter(typeof(AbsenceReason)).ConvertFromString(matchingSentralAbsence.Reason));
                    }
                }

                if (group.All(absence => absence.AbsenceReason == AbsenceReason.SchoolBusiness || absence.AbsenceReason == AbsenceReason.SharedEnrolment))
                {
                    justifiedAbsentDates.Add(group.Key, string.Join(",", group.Select(entry => entry.AbsenceReason.Name)));
                }

                absentDates.Add(group.Key, string.Join(",", group.Select(entry => entry.AbsenceReason.Name)));
            }

            // Take absent whole days from enrolledDays as presentDays
            int absentDays = absentDates.Count;
            int presentDays = enrolledDays - absentDays;
            int justifiedDays = justifiedAbsentDates.Count;

            // Calculate percentage with presentDays / enrolledDays
            decimal percentage = (decimal)presentDays / (decimal)enrolledDays;

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
                student.Id,
                student.Name,
                grade,
                offerings.Select(entry => entry.Name.Value).ToList(),
                enrolledDates.Value.Count,
                absentDates.Count,
                enrolledDates.Value.Count - absentDates.Count,
                (decimal)(enrolledDates.Value.Count - absentDates.Count) / (decimal)enrolledDates.Value.Count,
                justifiedAbsentDates.Count,
                absentDates.Count - justifiedAbsentDates.Count,
                enrolledDates.Value.Count - (absentDates.Count - justifiedAbsentDates.Count), 
                (decimal)(enrolledDates.Value.Count - (absentDates.Count - justifiedAbsentDates.Count)) / (decimal)enrolledDates.Value.Count,
                absentDates.Select(entry => $"{entry.Key.ToString("dd/MM/yyyy")} - {entry.Value}").ToList(),
                justifiedAbsentDates.Select(entry => $"{entry.Key.ToString("dd/MM/yyyy")} - {entry.Value}").ToList()));

            // Don't need to determine if a date is week a or week b as the timetable should be symmetrical for number of periods. Need to check that this holds true for Stage 6, as there may have been a P5 class on at random times.
        }

        var report = await _excelService.CreateSefAttendanceDataExport(records, default);

        timer.Stop();
        _logger.Information("Operation took {time}", timer.Elapsed.ToString(@"m\:ss\.fff"));

        return File(report, FileContentTypes.ExcelModernFile, "Sef Attendance Data.xlsx");
    }
}