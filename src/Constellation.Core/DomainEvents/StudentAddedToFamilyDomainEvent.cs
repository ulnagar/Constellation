namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;

public sealed record StudentAddedToFamilyDomainEvent(
    DomainEventId Id,
    StudentFamilyMembership Membership)
    : DomainEvent(Id);
