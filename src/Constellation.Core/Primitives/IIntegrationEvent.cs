namespace Constellation.Core.Primitives;

using Constellation.Core.Models.Identifiers;
using MediatR;
using System;

public interface IIntegrationEvent : INotification, IEvent
{
    public IntegrationEventId Id { get; init; }
    public DateOnly? DelayUntil { get; init; }
}