namespace Constellation.Core.Models.Families.Events;

using DomainEvents;
using Identifiers;

public sealed record ParentRemovedFromFamilyDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    ParentId ParentId,
    string EmailAddress)
    : DomainEvent(Id);
