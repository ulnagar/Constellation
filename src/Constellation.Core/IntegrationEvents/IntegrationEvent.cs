namespace Constellation.Core.IntegrationEvents;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using System;

public abstract record IntegrationEvent(
    IntegrationEventId Id,
    DateOnly? DelayUntil = null)
    : IIntegrationEvent;