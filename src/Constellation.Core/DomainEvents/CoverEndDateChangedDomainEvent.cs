namespace Constellation.Core.DomainEvents;

using System;

public sealed record CoverEndDateChangedDomainEvent(
    Guid Id,
    Guid CoverId,
    DateOnly PreviousEndDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);