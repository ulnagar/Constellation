namespace Constellation.Core.Models.Assets.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AssetId(Guid Value)
    : IStronglyTypedId
{
    public static readonly AssetId Empty = new(Guid.Empty);

    public static AssetId FromValue(Guid value) =>
        new(value);

    public AssetId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}