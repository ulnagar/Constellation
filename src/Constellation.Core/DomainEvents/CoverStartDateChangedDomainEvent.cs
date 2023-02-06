namespace Constellation.Core.DomainEvents;

using System;

public sealed record CoverStartDateChangedDomainEvent(
    Guid Id,
    DateOnly PreviousStartDate,
    DateOnly NewStartDate)
    : DomainEvent(Id);
