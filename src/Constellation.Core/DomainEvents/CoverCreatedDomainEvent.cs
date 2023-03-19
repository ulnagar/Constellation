namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record CoverCreatedDomainEvent(
    DomainEventId Id,
    ClassCoverId CoverId)
    : DomainEvent(Id);
