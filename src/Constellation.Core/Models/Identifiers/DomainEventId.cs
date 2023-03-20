namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record DomainEventId(Guid Value)
{
    public DomainEventId()
        : this(Guid.NewGuid()) { }
}