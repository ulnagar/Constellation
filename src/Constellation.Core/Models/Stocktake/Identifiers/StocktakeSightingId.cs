namespace Constellation.Core.Models.Stocktake.Identifiers;

using Primitives;
using System;

public readonly record struct StocktakeSightingId(Guid Value)
    : IStronglyTypedId
{
    public static readonly StocktakeSightingId Empty = new(Guid.Empty);

    public static StocktakeSightingId FromValue(Guid value) =>
        new(value);

    public StocktakeSightingId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}