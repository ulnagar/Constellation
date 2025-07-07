namespace Constellation.Core.Models.Stocktake.Identifiers;

using Primitives;
using System;

public readonly record struct DifferenceId(Guid Value)
    : IStronglyTypedId
{
    public static readonly DifferenceId Empty = new(Guid.Empty);

    public static DifferenceId FromValue(Guid value) =>
        new(value);

    public DifferenceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}