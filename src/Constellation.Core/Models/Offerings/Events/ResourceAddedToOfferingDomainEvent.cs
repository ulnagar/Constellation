namespace Constellation.Core.Models.Offerings.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;

public sealed record ResourceAddedToOfferingDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    ResourceId ResourceId,
    ResourceType ResourceType)
    : DomainEvent(Id);