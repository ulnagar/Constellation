namespace Constellation.Core.DomainEvents;

using System;

public sealed record CoverStartAndEndDatesChangedDomainEvent(
    Guid Id,
    Guid CoverId,
    DateOnly PreviousStartDate,
    DateOnly PreviousEndDate,
    DateOnly NewStartDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);
