namespace Constellation.Core.Models.Assets.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SightingId(Guid Value)
    : IStronglyTypedId
{
    public static readonly SightingId Empty = new(Guid.Empty);

    public static SightingId FromValue(Guid value) =>
        new(value);

    public SightingId()
        : this(Guid.NewGuid()) { }

    public override string ToString() => Value.ToString();
}