namespace Constellation.Core.DomainEvents;

using Constellation.Core.Primitives;
using System;

public abstract record DomainEvent(Guid Id) : IDomainEvent;
