namespace Constellation.Core.Models.Identifiers;

using System;

public record struct ClassCoverId(Guid Value)
{
    public static ClassCoverId Empty => new(Guid.Empty);

    public static ClassCoverId FromValue(Guid value) =>
        new(value);

    public ClassCoverId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}