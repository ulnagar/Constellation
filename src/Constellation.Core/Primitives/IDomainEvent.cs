namespace Constellation.Core.Primitives;

using MediatR;
using System;

public interface IDomainEvent : INotification
{
    public Guid Id { get; init; }
}
