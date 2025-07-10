namespace Constellation.Core.Models.Covers.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct CoverId(Guid Value)
    : IStronglyTypedId
{
    public static CoverId Empty => new(Guid.Empty);

    public static CoverId FromValue(Guid value) =>
        new(value);

    public CoverId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}