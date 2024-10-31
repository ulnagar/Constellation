namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using System;

public abstract record DomainEvent(
    DomainEventId Id,
    DateOnly? DelayUntil = null)
    : IDomainEvent;