namespace Constellation.Core.Models.Families.Events;

using DomainEvents;
using Families;
using Identifiers;

public sealed record StudentRemovedFromFamilyDomainEvent(
    DomainEventId Id,
    StudentFamilyMembership Membership)
    : DomainEvent(Id);