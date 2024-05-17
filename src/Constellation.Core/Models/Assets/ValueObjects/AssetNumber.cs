namespace Constellation.Core.Models.Assets.ValueObjects;

using Errors;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;

public sealed class AssetNumber : ValueObject, IComparable
{
    public string Number { get; }

    private AssetNumber(string number) => Number = number;

    public static Result<AssetNumber> Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure<AssetNumber>(AssetNumberErrors.Empty);

        number = number.TrimStart(' ', 'A', 'C', '0');

        if (number.Length > 8)
            return Result.Failure<AssetNumber>(AssetNumberErrors.TooLong);

        if (!int.TryParse(number, out _))
            return Result.Failure<AssetNumber>(AssetNumberErrors.UnknownCharacters);

        AssetNumber assetNumber = new($"AC{number.PadLeft(8, '0')}");

        return assetNumber;
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
    }

    public int CompareTo(object? obj)
    {
        if (obj is AssetNumber other)
            return string.Compare(Number, other.Number, StringComparison.Ordinal);

        return -1;
    }

    private bool Equals(AssetNumber other) => base.Equals(other) && Number == other.Number;
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is AssetNumber other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Number);
    public static bool operator ==(AssetNumber left, AssetNumber right) => EqualOperator(left, right);
    public static bool operator !=(AssetNumber left, AssetNumber right) => NotEqualOperator(left, right);

    public static bool operator <(AssetNumber? left, AssetNumber? right) => 
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(AssetNumber? left, AssetNumber? right) => 
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >(AssetNumber? left, AssetNumber? right) => 
        left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(AssetNumber? left, AssetNumber? right) => 
        left is not null && left.CompareTo(right) >= 0;
}
