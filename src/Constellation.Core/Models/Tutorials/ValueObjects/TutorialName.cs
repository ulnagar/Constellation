namespace Constellation.Core.Models.Tutorials.ValueObjects;

using Primitives;
using System;

public sealed class TutorialName : ValueObject<TutorialName, string>, IValueObject<TutorialName, string>, IComparable
{
    public static TutorialName Empty => new(string.Empty);

    private TutorialName(string value)
    {
        Value = value;
    }

    public static TutorialName FromValue(string value) 
        => new(value);

    public override string ToString() => Value;

    public int CompareTo(object? obj)
    {
        if (obj is TutorialName other)
        {
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        return -1;
    }

    public static implicit operator string(TutorialName? offeringName) =>
        offeringName is null ? string.Empty : offeringName.ToString();
}
