namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Abstractions.Repositories;
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
using Serilog;

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
        _logger = logger;
    }

    public async Task OnGet()
    {
        DateOnly startDate = new(2023, 01, 01);
        DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        DateOnly endDate = new(2023, 7, 1);
        DateTime endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

        List<Student> students = await _studentRepository.GetEnrolledForDates(startDate, endDate, default);

        List<Data> records = [];

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

            int enrolledDays = enrolledDates.Value.Count();
            List<DateOnly> absentDates = [];

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

                absentDates.Add(group.Key);
            }

            // Take absent whole days from enrolledDays as presentDays
            int absentDays = absentDates.Count;
            int presentDays = enrolledDays - absentDays;

            // Calculate percentage with presentDays / enrolledDays
            decimal percentage = (decimal)presentDays / (decimal)enrolledDays;

            records.Add(new(
                student.Id,
                student.Name,
                enrolledDays,
                absentDays,
                presentDays, 
                percentage, 
                absentDates));

            // Don't need to determine if a date is week a or week b as the timetable should be symmetrical for number of periods. Need to check that this holds true for Stage 6, as there may have been a P5 class on at random times.
        }

        return;
    }

    public record Data(
        StudentId StudentId,
        Name Student,
        int EnrolledDays,
        int AbsentDays,
        int PresentDays,
        decimal Percentage,
        List<DateOnly> AbsentDates);
}