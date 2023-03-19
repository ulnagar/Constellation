namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record CoverStartAndEndDatesChangedDomainEvent(
    DomainEventId Id,
    ClassCoverId CoverId,
    DateOnly PreviousStartDate,
    DateOnly PreviousEndDate,
    DateOnly NewStartDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);
