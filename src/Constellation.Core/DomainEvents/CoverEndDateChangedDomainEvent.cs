namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record CoverEndDateChangedDomainEvent(
    DomainEventId Id,
    ClassCoverId CoverId,
    DateOnly PreviousEndDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);