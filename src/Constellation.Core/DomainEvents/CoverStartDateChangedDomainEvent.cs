namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record CoverStartDateChangedDomainEvent(
    DomainEventId Id,
    ClassCoverId CoverId,
    DateOnly PreviousStartDate,
    DateOnly NewStartDate)
    : DomainEvent(Id);
