namespace Constellation.Core.Models.StaffMembers.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.StaffMembers.Identifiers;

public sealed record StaffMemberCreatedDomainEvent(
    DomainEventId Id,
    StaffId StaffId)
    : DomainEvent(Id);