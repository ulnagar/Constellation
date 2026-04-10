namespace Constellation.Core.Models.Offerings.ValueObjects;

using Primitives;
using System;

public sealed class OfferingName : ValueObject<OfferingName, string>, IValueObject<OfferingName, string>, IComparable
{
    public static OfferingName Empty => new(string.Empty);

    private OfferingName(string value)
    {
        Value = value;
    }

    public static OfferingName FromValue(string value) 
    {
        if (string.IsNullOrEmpty(value))
            return Empty;

        return new OfferingName(value);
    }

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is OfferingName other)
        {
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        return -1;
    }

    public static implicit operator string(OfferingName? offeringName) =>
        offeringName is null ? string.Empty : offeringName.ToString();
}
