namespace Constellation.Core.Models.Covers.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record CoverStartDateChangedDomainEvent(
    DomainEventId Id,
    CoverId CoverId,
    DateOnly PreviousStartDate,
    DateOnly NewStartDate)
    : DomainEvent(Id);
