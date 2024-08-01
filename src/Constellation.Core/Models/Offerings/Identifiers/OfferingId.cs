namespace Constellation.Core.Models.Offerings.Identifiers;

using System;

public record struct OfferingId(Guid Value)
{
    public static OfferingId Empty => new(Guid.Empty);

    public static OfferingId FromValue(Guid value) =>
        new(value);

    public OfferingId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
