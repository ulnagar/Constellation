namespace Constellation.Application.Attendance.Plans.SubmitAttendancePlan;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SubmitAttendancePlanCommandHandler
: ICommandHandler<SubmitAttendancePlanCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
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

        plan.SubmitPlan($"Plan submitted by {_currentUserService.UserName}");

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
