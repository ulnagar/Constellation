namespace Constellation.Core.Models.Offerings.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record SessionDeletedDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    int SessionId)
    : DomainEvent(Id);
