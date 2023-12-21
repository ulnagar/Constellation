namespace Constellation.Core.Models.Families.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;

public sealed record StudentRemovedFromFamilyDomainEvent(
    DomainEventId Id,
    StudentFamilyMembership Membership)
    : DomainEvent(Id);