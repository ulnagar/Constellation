namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct ClassCoverId(Guid Value)
    : IStronglyTypedId
{
    public static ClassCoverId Empty => new(Guid.Empty);

    public static ClassCoverId FromValue(Guid value) =>
        new(value);

    public ClassCoverId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}