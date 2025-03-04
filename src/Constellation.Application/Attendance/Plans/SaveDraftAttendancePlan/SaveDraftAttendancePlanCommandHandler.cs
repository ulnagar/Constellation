namespace Constellation.Application.Attendance.Plans.SaveDraftAttendancePlan;

using Abstractions.Messaging;
using Constellation.Core.Models.Timetables.Enums;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Identifiers;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SaveDraftAttendancePlanCommandHandler
: ICommandHandler<SaveDraftAttendancePlanCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SaveDraftAttendancePlanCommandHandler(
        IAttendancePlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<SaveDraftAttendancePlanCommand>();
    }

    public async Task<Result> Handle(SaveDraftAttendancePlanCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(SaveDraftAttendancePlanCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to update Attendance Plan with supplied times");

            return Result.Failure(AttendancePlanErrors.NotFound(request.PlanId));
        }

        List<(AttendancePlanPeriodId, TimeOnly, TimeOnly)> updateEntries = new();

        foreach (SaveDraftAttendancePlanCommand.PlanPeriod periodDetails in request.Periods)
        {
            AttendancePlanPeriod period = plan.Periods.FirstOrDefault(period => period.Id == periodDetails.PlanPeriodId);

            if (period is null)
            {
                // Does not match
                _logger
                    .ForContext(nameof(SaveDraftAttendancePlanCommand), request, true)
                    .ForContext(nameof(SaveDraftAttendancePlanCommand.PlanPeriod), periodDetails, true)
                    .ForContext(nameof(AttendancePlan), plan, true)
                    .ForContext(nameof(Error), AttendancePlanErrors.PeriodNotFound(periodDetails.PlanPeriodId), true)
                    .Warning("Failed to update Attendance Plan with supplied times");

                return Result.Failure(AttendancePlanErrors.PeriodNotFound(periodDetails.PlanPeriodId));
            }

            updateEntries.Add(new(periodDetails.PlanPeriodId, periodDetails.EntryTime, periodDetails.ExitTime));
        }

        Result updateAttempt = plan.UpdatePeriods(updateEntries, _currentUserService, _dateTime);

        if (updateAttempt.IsFailure)
        {
            _logger
                .ForContext(nameof(SaveDraftAttendancePlanCommand), request, true)
                .ForContext(nameof(AttendancePlan), plan, true)
                .ForContext(nameof(Error), updateAttempt.Error, true)
                .Warning("Failed to update Attendance Plan with supplied times");

            return Result.Failure(updateAttempt.Error);
        }

        if (request.SciencePracLesson is not null)
        {
            Result updateScienceLesson = plan.UpdateSciencePracLesson(
                request.SciencePracLesson.Week,
                request.SciencePracLesson.Day,
                request.SciencePracLesson.Period);

            if (updateScienceLesson.IsFailure)
            {
                _logger
                    .ForContext(nameof(SaveDraftAttendancePlanCommand), request, true)
                    .ForContext(nameof(AttendancePlan), plan, true)
                    .ForContext(nameof(Error), updateScienceLesson.Error, true)
                    .Warning("Failed to update Attendance Plan with supplied times");

                return Result.Failure(updateScienceLesson.Error);
            }
        }

        List<(string, double, double)> missedLessons = request.MissedLessons
            .Select(entry => (entry.Subject, entry.TotalMinutesPerCycle, entry.MinutesMissedPerCycle))
            .ToList();

        plan.AddMissedLessons(missedLessons);

        List<(PeriodWeek Week, PeriodDay Day, string Period, double Minutes, string Activity)> freePeriods = request.FreePeriods
            .Select(entry => (entry.Week, entry.Day, entry.Period, entry.Minutes, entry.Activity))
            .ToList();

        plan.AddFreePeriods(freePeriods);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
