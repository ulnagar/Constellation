namespace Constellation.Core.Models.Assets.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct LocationId(Guid Value)
    : IStronglyTypedId
{
    public static readonly LocationId Empty = new(Guid.Empty);

    public static LocationId FromValue(Guid value) =>
        new(value);

    public LocationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() => Value.ToString();
}