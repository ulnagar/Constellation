namespace Constellation.Application.Attendance.Plans.GetAttendancePlanForSubmit;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Identifiers;
using Core.Models.Attendance.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendancePlanForSubmitQueryHandler
: IQueryHandler<GetAttendancePlanForSubmitQuery, AttendancePlanEntry>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public GetAttendancePlanForSubmitQueryHandler(
        IAttendancePlanRepository planRepository,
        IPeriodRepository periodRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _periodRepository = periodRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<GetAttendancePlanForSubmitQuery>();
    }

    public async Task<Result<AttendancePlanEntry>> Handle(GetAttendancePlanForSubmitQuery request, CancellationToken cancellationToken)
    {
        AttendancePlan plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(GetAttendancePlanForSubmitQuery), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to retrieve attendance plan");

            return Result.Failure<AttendancePlanEntry>(AttendancePlanErrors.NotFound(request.PlanId));
        }

        if (plan.Status == AttendancePlanStatus.Processing)
        {
            plan.EditPlan($"Plan opened for editing by {_currentUserService.UserName}");

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        List<Period> timetablePeriods = await _periodRepository.GetAllFromTimetable([plan.Periods.First().Timetable], cancellationToken);

        List<AttendancePlanEntry.PlanPeriod> periods = new();

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

        List<AttendancePlanEntry.FreePeriod> freePeriods = new();

        foreach (var period in plan.FreePeriods)
        {
            freePeriods.Add(new(
                period.Week,
                period.Day,
                period.Period,
                period.Minutes,
                period.Activity));
        }

        List<AttendancePlanEntry.MissedPeriod> missedPeriods = new();

        foreach (var period in plan.MissedLessons)
        {
            missedPeriods.Add(new(
                period.Subject,
                period.TotalMinutesPerCycle,
                period.MinutesMissedPerCycle,
                period.PercentMissed));
        }

        AttendancePlanEntry.SciencePracLesson? scienceLesson = (plan.SciencePracLesson is not null)
            ? new(
                plan.SciencePracLesson.Week,
                plan.SciencePracLesson.Day,
                plan.SciencePracLesson.Period)
            : null;

        AttendancePlanEntry response = new(
            plan.Id,
            plan.Status,
            plan.StudentId,
            plan.Student,
            plan.Grade,
            plan.SchoolCode,
            plan.School,
            periods,
            freePeriods,
            missedPeriods,
            scienceLesson);

        return response;
    }
}
