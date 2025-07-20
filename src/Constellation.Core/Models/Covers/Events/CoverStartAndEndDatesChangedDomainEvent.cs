namespace Constellation.Core.Models.Covers.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record CoverStartAndEndDatesChangedDomainEvent(
    DomainEventId Id,
    CoverId CoverId,
    DateOnly PreviousStartDate,
    DateOnly PreviousEndDate,
    DateOnly NewStartDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);
