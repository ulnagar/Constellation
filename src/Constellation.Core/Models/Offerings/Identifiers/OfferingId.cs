namespace Constellation.Core.Models.Offerings.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct OfferingId(Guid Value)
    : IStronglyTypedId
{
    public static OfferingId Empty => new(Guid.Empty);

    public static OfferingId FromValue(Guid value) =>
        new(value);

    public OfferingId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
