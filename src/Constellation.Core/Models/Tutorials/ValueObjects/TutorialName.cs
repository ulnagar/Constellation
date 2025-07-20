namespace Constellation.Core.Models.Tutorials.ValueObjects;

using Primitives;
using System;
using System.Collections.Generic;

public sealed class TutorialName : ValueObject, IComparable
{
    public static TutorialName None => new("");

    private TutorialName(string value)
    {
        Value = value;
    }

    public static TutorialName FromValue(string value) 
        => new TutorialName(value);

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public int CompareTo(object obj)
    {
        if (obj is TutorialName other)
        {
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        return -1;
    }

    public static implicit operator string(TutorialName offeringName) =>
        offeringName is null ? string.Empty : offeringName.ToString();
}
