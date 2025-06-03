namespace Constellation.Core.Models.StaffMembers.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record StaffMemberResignedDomainEvent(
    DomainEventId Id,
    StaffId StaffId)
    : DomainEvent(Id);