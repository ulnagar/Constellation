namespace Constellation.Core.Primitives;

using Constellation.Core.Models.Identifiers;
using MediatR;

public interface IDomainEvent : INotification
{
    public DomainEventId Id { get; init; }
}
