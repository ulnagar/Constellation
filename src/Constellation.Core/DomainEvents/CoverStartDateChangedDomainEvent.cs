namespace Constellation.Core.DomainEvents;

using System;

public sealed record CoverStartDateChangedDomainEvent(
    Guid Id,
    Guid CoverId,
    DateOnly PreviousStartDate,
    DateOnly NewStartDate)
    : DomainEvent(Id);
