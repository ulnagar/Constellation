namespace Constellation.Core.Models.Offerings.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Enums;
using Identifiers;
using ValueObjects;

public sealed record ResourceAddedToOfferingDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    ResourceId ResourceId,
    ResourceType ResourceType)
    : DomainEvent(Id);