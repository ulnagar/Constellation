namespace Constellation.Core.Models.Stocktake.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct StocktakeEventId(Guid Value)
    : IStronglyTypedId
{
    public static readonly StocktakeEventId Empty = new(Guid.Empty);

    public static StocktakeEventId FromValue(Guid value) =>
        new(value);

    public StocktakeEventId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}