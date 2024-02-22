namespace Constellation.Core.Models.Offerings.Events;

using DomainEvents;
using Constellation.Core.Models.Identifiers;
using Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record ResourceRemovedFromOfferingDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    Resource Resource)
    : DomainEvent(Id);