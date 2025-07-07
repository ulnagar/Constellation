namespace Constellation.Core.Models.StaffMembers.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record StaffMemberEmailAddressChangedDomainEvent(
    DomainEventId Id,
    StaffId StaffId,
    string OldEmailAddress,
    string NewEmailAddress)
    : DomainEvent(Id);