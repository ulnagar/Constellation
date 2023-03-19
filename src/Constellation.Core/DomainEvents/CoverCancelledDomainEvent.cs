using Constellation.Core.Models.Identifiers;
using System;

namespace Constellation.Core.DomainEvents;

public sealed record CoverCancelledDomainEvent(
    DomainEventId Id,
    ClassCoverId CoverId)
    : DomainEvent(Id);
