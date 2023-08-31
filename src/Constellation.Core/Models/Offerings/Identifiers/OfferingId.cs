namespace Constellation.Core.Models.Offerings.Identifiers;

using System;

public sealed record OfferingId(Guid Value)
{
    public static OfferingId FromValue(Guid Value) =>
        new(Value);

    public OfferingId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
