namespace Constellation.Core.Models.Covers.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record CoverEndDateChangedDomainEvent(
    DomainEventId Id,
    CoverId CoverId,
    DateOnly PreviousEndDate,
    DateOnly NewEndDate)
    : DomainEvent(Id);