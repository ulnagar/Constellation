namespace Constellation.Core.DomainEvents;

using System;

public sealed record CoverCreatedDomainEvent(
    Guid Id,
    Guid CoverId)
    : DomainEvent(Id);
