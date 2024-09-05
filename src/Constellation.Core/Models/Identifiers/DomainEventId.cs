namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record DomainEventId(Guid Value)
    : IStronglyTypedId
{
    public static DomainEventId FromValue(Guid value) =>
        new(value);

    public DomainEventId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}