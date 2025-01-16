namespace Constellation.Application.Attendance.Plans.GetAttendancePlanDetails;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Models.Timetables;
using Constellation.Core.Models.Timetables.Repositories;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects;
using Core.Models.Timetables.Identifiers;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendancePlanDetailsQueryHandler
: IQueryHandler<GetAttendancePlanDetailsQuery, AttendancePlanDetailsResponse>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetAttendancePlanDetailsQueryHandler(
        IAttendancePlanRepository planRepository,
        IPeriodRepository periodRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        ILogger logger)
    {
        _planRepository = planRepository;
        _periodRepository = periodRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _logger = logger
            .ForContext<GetAttendancePlanDetailsQuery>();
    }

    public async Task<Result<AttendancePlanDetailsResponse>> Handle(GetAttendancePlanDetailsQuery request, CancellationToken cancellationToken)
    {
        AttendancePlan plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(GetAttendancePlanDetailsQuery), request, true)
            .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to find Attendance Plan with Id {id}", request.PlanId);

            return Result.Failure<AttendancePlanDetailsResponse>(AttendancePlanErrors.NotFound(request.PlanId));
        }

        List<Period> timetablePeriods = await _periodRepository.GetAllFromTimetable([plan.Periods.First().Timetable], cancellationToken);

        List<AttendancePlanDetailsResponse.PlanPeriod> periods = new();

        foreach (var period in timetablePeriods)
        {
            AttendancePlanPeriod planPeriod = plan.Periods.FirstOrDefault(planPeriod => planPeriod.PeriodId == period.Id);

            if (planPeriod is null)
            {
                periods.Add(new(
                    AttendancePlanPeriodId.Empty,
                    period.Timetable,
                    period.Week,
                    period.Day,
                    period.Name,
                    period.Type,
                    TimeOnly.FromTimeSpan(period.StartTime),
                    TimeOnly.FromTimeSpan(period.EndTime),
                    string.Empty,
                    string.Empty,
                    TimeOnly.MinValue,
                    TimeOnly.MinValue));
            }
            else
            {
                periods.Add(new(
                    planPeriod.Id,
                    period.Timetable,
                    period.Week,
                    period.Day,
                    period.Name,
                    period.Type,
                    TimeOnly.FromTimeSpan(period.StartTime),
                    TimeOnly.FromTimeSpan(period.EndTime),
                    planPeriod.OfferingName,
                    planPeriod.CourseName,
                    planPeriod.EntryTime,
                    planPeriod.ExitTime));
            }
        }

        List<AttendancePlanDetailsResponse.FreePeriod> freePeriods = new();

        foreach (var period in plan.FreePeriods)
        {
            freePeriods.Add(new(
                period.Week,
                period.Day,
                period.Period,
                period.Minutes,
                period.Activity));
        }

        List<AttendancePlanDetailsResponse.MissedPeriod> missedPeriods = new();

        foreach (var period in plan.MissedLessons)
        {
            missedPeriods.Add(new(
                period.Subject,
                period.TotalMinutesPerCycle,
                period.MinutesMissedPerCycle,
                period.PercentMissed));
        }

        AttendancePlanDetailsResponse.SciencePracLesson? scienceLesson = (plan.SciencePracLesson is not null)
            ? new(
                plan.SciencePracLesson.Week,
                plan.SciencePracLesson.Day,
                plan.SciencePracLesson.Period)
            : null;

        List<AttendancePlanDetailsResponse.NoteDetails> notes = new();

        foreach (var note in plan.Notes)
        {
            notes.Add(new(
                note.Id,
                note.Message,
                note.CreatedBy,
            note.CreatedAt));
        }

        List<AttendancePlanDetailsResponse.AlternatePercentage> alternatePercentages = new();

        OfferingId offeringId = plan.Periods
            .GroupBy(entry => entry.OfferingId)
            .OrderByDescending(group => group.Count())
            .First()
            .Key;

        List<Offering> offerings = await _offeringRepository.GetOfferingsFromSameGroup(offeringId, cancellationToken);

        foreach (Offering offering in offerings.OrderBy(offering => offering.Name))
        {
            Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            IEnumerable<PeriodId> periodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId);

            double total = 0;

            foreach (PeriodId periodId in periodIds)
            {
                AttendancePlanPeriod matchingPeriod = plan.Periods.SingleOrDefault(period => period.PeriodId == periodId);

                if (matchingPeriod is null)
                    continue;

                total += matchingPeriod.MinutesPresent;
            }

            alternatePercentages.Add(new(
                course.Name,
                offering.Name,
                total,
                (total / course.TargetMinutesPerCycle)));
        }

        AttendancePlanDetailsResponse response = new(
            plan.Id,
            plan.Status,
            plan.StudentId,
            plan.Student,
            plan.Grade,
            plan.SchoolCode,
            plan.School,
            notes,
            periods,
            freePeriods,
            missedPeriods,
            scienceLesson,
            alternatePercentages);

        return response;
    }
}
