namespace Constellation.Application.Attendance.Plans.CopyAttendancePlanDetails;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CopyAttendancePlanDetailsCommandHandler
: ICommandHandler<CopyAttendancePlanDetailsCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CopyAttendancePlanDetailsCommandHandler(
        IAttendancePlanRepository planRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CopyAttendancePlanDetailsCommand>();
    }

    public async Task<Result> Handle(CopyAttendancePlanDetailsCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan sourcePlan = await _planRepository.GetById(request.SourcePlanId, cancellationToken);

        if (sourcePlan is null)
        {
            _logger
                .ForContext(nameof(CopyAttendancePlanDetailsCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.SourcePlanId), true)
                .Warning("Failed to retrieve Source Attendance Plan for copy");

            return Result.Failure(AttendancePlanErrors.NotFound(request.SourcePlanId));
        }

        AttendancePlan destinationPlan = await _planRepository.GetById(request.DestinationPlanId, cancellationToken);

        if (destinationPlan is null)
        {
            _logger
                .ForContext(nameof(CopyAttendancePlanDetailsCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.DestinationPlanId), true)
                .Warning("Failed to retrieve Destination Attendance Plan for copy");

            return Result.Failure(AttendancePlanErrors.NotFound(request.DestinationPlanId));
        }

        foreach (var period in destinationPlan.Periods)
        {
            AttendancePlanPeriod sourcePeriod = sourcePlan.Periods.FirstOrDefault(entry => entry.PeriodId == period.PeriodId);

            if (sourcePeriod is null)
                continue;

            destinationPlan.UpdatePeriods(
                [(period.Id, sourcePeriod.EntryTime, sourcePeriod.ExitTime)],
                _currentUserService,
                _dateTime);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
