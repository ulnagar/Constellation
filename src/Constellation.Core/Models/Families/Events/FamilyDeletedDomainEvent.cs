namespace Constellation.Core.Models.Families.Events;

using DomainEvents;
using Identifiers;

public sealed record FamilyDeletedDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId)
    : DomainEvent(Id);