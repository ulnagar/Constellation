namespace Constellation.Core.Models.Attendance.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Identifiers;

public sealed record AttendancePlanRejectedDomainEvent(
    DomainEventId Id,
    AttendancePlanId PlanId)
    : DomainEvent(Id);