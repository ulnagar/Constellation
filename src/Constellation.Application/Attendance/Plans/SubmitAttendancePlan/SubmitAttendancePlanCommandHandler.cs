namespace Constellation.Application.Attendance.Plans.SubmitAttendancePlan;

using Abstractions.Messaging;
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

internal sealed class SubmitAttendancePlanCommandHandler
: ICommandHandler<SubmitAttendancePlanCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SubmitAttendancePlanCommandHandler(
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
            .ForContext<SubmitAttendancePlanCommand>();
    }

    public async Task<Result> Handle(SubmitAttendancePlanCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(SubmitAttendancePlanCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to update Attendance Plan with supplied times");

            return Result.Failure(AttendancePlanErrors.NotFound(request.PlanId));
        }

        List<(AttendancePlanPeriodId, TimeOnly, TimeOnly)> updateEntries = new List<(AttendancePlanPeriodId, TimeOnly, TimeOnly)>();

        foreach (SubmitAttendancePlanCommand.PlanPeriod periodDetails in request.Periods)
        {
            AttendancePlanPeriod period = plan.Periods.FirstOrDefault(period => period.Id == periodDetails.PlanPeriodId);

            if (period is null)
            {
                // Does not match
                _logger
                    .ForContext(nameof(SubmitAttendancePlanCommand), request, true)
                    .ForContext(nameof(SubmitAttendancePlanCommand.PlanPeriod), periodDetails, true)
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
                .ForContext(nameof(SubmitAttendancePlanCommand), request, true)
                .ForContext(nameof(AttendancePlan), plan, true)
                .ForContext(nameof(Error), updateAttempt.Error, true)
                .Warning("Failed to update Attendance Plan with supplied times");

            return Result.Failure(updateAttempt.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
