namespace Constellation.Application.Domains.Attendance.Plans.Events.AttendancePlanAccepted;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Models.Timetables.Enums;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Errors;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Events;
using Core.Models.Attendance.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Extensions;
using LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using Serilog;

internal sealed class CreateUnavailabilitiesInSentral
    : IDomainEventHandler<AttendancePlanAcceptedDomainEvent>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _sentralGateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public CreateUnavailabilitiesInSentral(
        IAttendancePlanRepository planRepository,
        IStudentRepository studentRepository,
        ISentralGateway sentralGateway,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _planRepository = planRepository;
        _studentRepository = studentRepository;
        _sentralGateway = sentralGateway;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<AttendancePlanAcceptedDomainEvent>();
    }

    private sealed class Unavailability
    {
        public required PeriodWeek Week { get; init; }
        public required PeriodDay Day { get; init; }
        public required TimeOnly Start { get; init; }
        public required TimeOnly End { get; init; }
    }

    public async Task Handle(AttendancePlanAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        AttendancePlan? plan = await _planRepository.GetById(notification.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(notification.PlanId), true)
                .Warning("Failed to create Sentral Unavailabilities from attendance plan");

            return;
        }

        Student? student = await _studentRepository.GetById(plan.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(plan.StudentId), true)
                .Warning("Failed to create Sentral Unavailabilities from attendance plan");

            return;
        }

        string? sentralStudentId = student.SystemLinks
            .FirstOrDefault(entry => entry.System == SystemType.Sentral)?.Value;

        if (sentralStudentId is null)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), SystemLinkErrors.NotFound(SystemType.Sentral), true)
                .Warning("Failed to create Sentral Unavailabilities from attendance plan");

            return;
        }

        List<Unavailability> unavailabilities = [];

        foreach (AttendancePlanPeriod period in plan.Periods)
        {
            if (period.EntryTime != period.StartTime)
            {
                unavailabilities.Add(new()
                {
                    Week = period.Week,
                    Day = period.Day,
                    Start = period.StartTime,
                    End = period.EntryTime
                });
            }

            if (period.ExitTime != period.EndTime)
            {
                unavailabilities.Add(new()
                {
                    Week = period.Week,
                    Day = period.Day,
                    Start = period.ExitTime,
                    End = period.EndTime
                });
            }
        }

        // Remove any current Unavailabilities first
        Result remove = await _sentralGateway.RemoveStudentUnavailabilities(sentralStudentId, _dateTime.Today, _dateTime.LastDayOfCurrentYear, cancellationToken);

        if (remove.IsFailure)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), remove.Error, true)
                .Warning("Failed to create Sentral Unavailabilities from attendance plan");

            return;
        }

        // Create new Unavailabilities
        List<SchoolCalendarWeek> validWeeks = await _sentralGateway.GetTermsAndWeeksFromApi(plan.CreatedAt.Year.ToString(), cancellationToken);
        
        foreach (Unavailability entry in unavailabilities)
        {
            DateOnly firstDate = GetFirstDate(entry);
            List<DateOnly> validDates = GetValidDates(firstDate, validWeeks).ToList();

            Result create = await _sentralGateway.AddStudentUnavailabilities(sentralStudentId, validDates, entry.Start, entry.End, cancellationToken);

            if (create.IsFailure)
            {
                _logger
                    .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                    .ForContext(nameof(Unavailability), entry, true)
                    .ForContext(nameof(Error), create.Error, true)
                    .Warning("Failed to create Sentral Unavailabilities from attendance plan");
            }
        }
    }

    private DateOnly GetFirstDate(Unavailability entry)
    {
        DateOnly today = _dateTime.Today;
        int todayNum = today.GetDayNumber();                      // 1..10
        int target = (entry.Week.Value - 1) * 5 + entry.Day.Value; // 1..10

        // School days until the target; 0 means today
        int schoolDays = ((target - todayNum) % 10 + 10) % 10;

        // Convert school days to calendar days by skipping weekends
        int weekdayIndex = (int)today.DayOfWeek - 1;               // Mon=0 .. Fri=4
        int calendarDays = schoolDays + 2 * ((weekdayIndex + schoolDays) / 5);

        return today.AddDays(calendarDays);
    }

    private static IEnumerable<DateOnly> GetValidDates(
        DateOnly firstDate,
        IEnumerable<SchoolCalendarWeek> weeks)
    {
        List<(DateOnly Start, DateOnly End)> ranges = weeks
            .Select(w => (
                Start: DateOnly.FromDateTime(w.StartDate),
                End: DateOnly.FromDateTime(w.EndDate)))
            .ToList();

        if (ranges.Count == 0)
            yield break;

        DateOnly lastDate = ranges.Max(r => r.End);

        for (DateOnly date = firstDate; date <= lastDate; date = date.AddDays(14))
        {
            if (ranges.Any(r => date >= r.Start && date <= r.End))
                yield return date;
        }
    }
}