namespace Constellation.Core.Models.Offerings.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct ResourceId(Guid Value)
    : IStronglyTypedId
{
    public static ResourceId Empty => new(Guid.Empty);

    public static ResourceId FromValue(Guid value) =>
        new(value);

    public ResourceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
