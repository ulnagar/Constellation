namespace Constellation.Core.DomainEvents;

using System;

public sealed record CoverEndDateChangedDomainEvent(
    Guid Id,
    DateOnly PreviousEndDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);