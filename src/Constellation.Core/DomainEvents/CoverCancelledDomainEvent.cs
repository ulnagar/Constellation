using System;

namespace Constellation.Core.DomainEvents;

public sealed record CoverCancelledDomainEvent(
    Guid Id,
    Guid CoverId)
    : DomainEvent(Id);
