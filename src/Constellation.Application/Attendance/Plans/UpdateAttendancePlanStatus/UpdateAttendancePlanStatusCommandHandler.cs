namespace Constellation.Application.Attendance.Plans.UpdateAttendancePlanStatus;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateAttendancePlanStatusCommandHandler
: ICommandHandler<UpdateAttendancePlanStatusCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAttendancePlanStatusCommandHandler(
        IAttendancePlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateAttendancePlanStatusCommand>();
    }

    public async Task<Result> Handle(UpdateAttendancePlanStatusCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan? plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(UpdateAttendancePlanStatusCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to retrieve Attendance Plan for status update");

            return Result.Failure(AttendancePlanErrors.NotFound(request.PlanId));
        }

        Result update = plan.UpdateStatus(request.NewStatus);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateAttendancePlanStatusCommand), request, true)
                .ForContext(nameof(AttendancePlan), plan, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update status for Attendance Plan");

            return update;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
