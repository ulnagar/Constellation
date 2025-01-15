namespace Constellation.Application.Attendance.Plans.Events.AttendancePlanRejected;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Attendance.Events;
using Constellation.Core.Models.Attendance.Repositories;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Shared;
using GenerateAttendancePlans;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateNewPlan
    : IDomainEventHandler<AttendancePlanRejectedDomainEvent>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public GenerateNewPlan(
        IAttendancePlanRepository planRepository,
        ISender mediator,
        ILogger logger)
    {
        _planRepository = planRepository;
        _mediator = mediator;
        _logger = logger
            .ForContext<AttendancePlanRejectedDomainEvent>();
    }

    public async Task Handle(AttendancePlanRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        AttendancePlan? plan = await _planRepository.GetById(notification.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(AttendancePlanRejectedDomainEvent), notification, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(notification.PlanId), true)
                .Warning("Failed to generate new Attendance Plan");

            return;
        }

        Result generate = await _mediator.Send(new GenerateAttendancePlansCommand(plan.StudentId, plan.SchoolCode, plan.Grade), cancellationToken);

        if (generate.IsFailure)
        {
            _logger
                .ForContext(nameof(AttendancePlanRejectedDomainEvent), notification, true)
                .ForContext(nameof(Error), generate.Error, true)
                .Warning("Failed to generate new Attendance Plan");
        }
    }
}
