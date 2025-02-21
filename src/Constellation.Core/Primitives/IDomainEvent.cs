﻿namespace Constellation.Core.Primitives;

using Constellation.Core.Models.Identifiers;
using MediatR;
using System;

public interface IDomainEvent : INotification, IEvent
{
    public DomainEventId Id { get; init; }
    public DateOnly? DelayUntil { get; init; }
}