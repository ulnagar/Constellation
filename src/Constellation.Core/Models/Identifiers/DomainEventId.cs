namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record DomainEventId(Guid Value)
{
    public static DomainEventId FromValue(Guid value) =>
        new(value);

    public DomainEventId()
        : this(Guid.NewGuid()) { }
}