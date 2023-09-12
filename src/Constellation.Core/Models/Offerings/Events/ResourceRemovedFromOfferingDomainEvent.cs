namespace Constellation.Core.Models.Offerings.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record ResourceRemovedFromOfferingDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    Resource Resource)
    : DomainEvent(Id);