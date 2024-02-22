namespace Constellation.Core.Models.Families.Events;

using DomainEvents;
using Identifiers;

public sealed record ParentAddedToFamilyDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    ParentId ParentId)
    : DomainEvent(Id);
