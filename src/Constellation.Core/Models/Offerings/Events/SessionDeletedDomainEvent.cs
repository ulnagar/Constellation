namespace Constellation.Core.Models.Offerings.Events;

using DomainEvents;
using Constellation.Core.Models.Identifiers;
using Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record SessionDeletedDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    SessionId SessionId)
    : DomainEvent(Id);
