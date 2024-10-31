namespace Constellation.Application.Abstractions.Messaging;

using Core.Primitives;
using MediatR;

public interface IIntegrationEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IIntegrationEvent;