namespace Constellation.Application.Abstractions.Messaging;

using Constellation.Core.Primitives;
using MediatR;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}