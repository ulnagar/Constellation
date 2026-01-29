namespace Constellation.Core.Models.Offerings.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record ResourceRemovedFromOfferingDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    Resource Resource)
    : DomainEvent(Id);