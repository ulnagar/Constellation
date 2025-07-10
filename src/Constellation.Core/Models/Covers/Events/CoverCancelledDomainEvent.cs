namespace Constellation.Core.Models.Covers.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Identifiers;

public sealed record CoverCancelledDomainEvent(
    DomainEventId Id,
    CoverId CoverId)
    : DomainEvent(Id);
