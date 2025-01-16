namespace Constellation.Core.Models.Attendance.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record AttendancePlanAcceptedDomainEvent(
    DomainEventId Id,
    AttendancePlanId PlanId)
    : DomainEvent(Id);