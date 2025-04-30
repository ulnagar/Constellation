namespace Constellation.Application.Domains.Attendance.Plans.Events.AttendancePlanAccepted;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Enums;
using Constellation.Core.Models.Attendance.Errors;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Attendance.Events;
using Core.Models.Attendance.Repositories;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Mark previously accepted Attendance Plans for the same student in the same calendar year as Superseded
/// </summary>
internal sealed class MarkOldPlansSuperseded
: IDomainEventHandler<AttendancePlanAcceptedDomainEvent>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkOldPlansSuperseded(
        IAttendancePlanRepository planRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AttendancePlanAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        AttendancePlan currentPlan = await _planRepository.GetById(notification.PlanId, cancellationToken);

        if (currentPlan is null)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(notification.PlanId), true)
                .Warning("Failed to update superseded Attendance Plans");

            return;
        }

        if (currentPlan.Status != AttendancePlanStatus.Accepted)
        {
            _logger
                .ForContext(nameof(AttendancePlanAcceptedDomainEvent), notification, true)
                .ForContext(nameof(Error), AttendancePlanErrors.StatusNotAccepted, true)
                .Warning("Failed to update superseded Attendance Plans");

            return;
        }

        // Mark previously accepted plan(s) as Superseded
        List<AttendancePlan> studentPlans = await _planRepository.GetForStudent(currentPlan.StudentId, cancellationToken);
        List<AttendancePlan> expiredPlans = studentPlans
            .Where(entry =>
                entry.CreatedAt.Year == _dateTime.CurrentYear &&
                entry.Status == AttendancePlanStatus.Accepted)
            .ToList();

        foreach (AttendancePlan expiredPlan in expiredPlans)
        {
            if (expiredPlan == currentPlan)
                continue;

            expiredPlan.SupersededBy(currentPlan.Id);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}