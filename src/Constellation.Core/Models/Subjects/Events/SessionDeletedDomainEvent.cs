namespace Constellation.Core.Models.Subjects.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record SessionDeletedDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    int SessionId)
    : DomainEvent(Id);
