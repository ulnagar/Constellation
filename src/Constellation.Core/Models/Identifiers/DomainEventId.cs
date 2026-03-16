namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct DomainEventId(Guid Value)
    : IStronglyTypedId
{
    public static DomainEventId FromValue(Guid value) =>
        new(value);

    public DomainEventId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}