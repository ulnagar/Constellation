namespace Constellation.Core.DomainEvents;

using System;

public sealed record CoverStartAndEndDatesChangedDomainEvent(
    Guid Id,
    DateOnly PreviousStartDate,
    DateOnly PreviousEndDate,
    DateOnly NewStartDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);
