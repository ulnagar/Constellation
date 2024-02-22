namespace Constellation.Core.Models.Offerings.Events;

using DomainEvents;
using Constellation.Core.Models.Identifiers;
using Identifiers;
using ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record ResourceAddedToOfferingDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    ResourceId ResourceId,
    ResourceType ResourceType)
    : DomainEvent(Id);