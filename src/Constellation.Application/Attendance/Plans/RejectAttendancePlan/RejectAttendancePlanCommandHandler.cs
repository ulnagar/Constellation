namespace Constellation.Application.Attendance.Plans.RejectAttendancePlan;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RejectAttendancePlanCommandHandler
: ICommandHandler<RejectAttendancePlanCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RejectAttendancePlanCommandHandler(
        IAttendancePlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RejectAttendancePlanCommand>();
    }

    public async Task<Result> Handle(RejectAttendancePlanCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan? plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(RejectAttendancePlanCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to retrieve Attendance Plan for status update");

            return Result.Failure(AttendancePlanErrors.NotFound(request.PlanId));
        }

        Result update = plan.RejectPlan(request.Comment, request.SendEmail);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(RejectAttendancePlanCommand), request, true)
                .ForContext(nameof(AttendancePlan), plan, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update status for Attendance Plan");

            return update;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
