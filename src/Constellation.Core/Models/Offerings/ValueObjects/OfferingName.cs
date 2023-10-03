namespace Constellation.Core.Models.Offerings.ValueObjects;

using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;

public sealed class OfferingName : ValueObject
{
    private OfferingName(string value)
    {
        Value = value;
    }

    public static Result<OfferingName> FromValue(string value) 
    {
        if (string.IsNullOrEmpty(value))
        {
            return Result.Failure<OfferingName>(OfferingNameErrors.ValueEmpty);
        }

        return new OfferingName(value);
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(OfferingName offeringName) =>
        offeringName is null ? string.Empty : offeringName.ToString();
}
