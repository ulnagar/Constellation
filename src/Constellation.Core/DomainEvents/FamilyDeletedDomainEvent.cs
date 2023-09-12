namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record FamilyDeletedDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId)
    : DomainEvent(Id);