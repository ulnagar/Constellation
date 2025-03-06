namespace Constellation.Application.Attendance.Plans.ApproveAttendancePlan;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ApproveAttendancePlanCommandHandler
: ICommandHandler<ApproveAttendancePlanCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ApproveAttendancePlanCommandHandler(
        IAttendancePlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ApproveAttendancePlanCommand>();
    }

    public async Task<Result> Handle(ApproveAttendancePlanCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan? plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(ApproveAttendancePlanCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to retrieve Attendance Plan for status update");

            return Result.Failure(AttendancePlanErrors.NotFound(request.PlanId));
        }

        Result update = plan.ApprovePlan(request.Comment);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(ApproveAttendancePlanCommand), request, true)
                .ForContext(nameof(AttendancePlan), plan, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update status for Attendance Plan");

            return update;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
