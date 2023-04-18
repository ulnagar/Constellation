namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record ClassCoverId(Guid Value)
{
    public static ClassCoverId FromValue(Guid value) =>
        new(value);

    public ClassCoverId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}