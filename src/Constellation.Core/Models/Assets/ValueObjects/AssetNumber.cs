namespace Constellation.Core.Models.Assets.ValueObjects;

using Errors;
using Primitives;
using Shared;
using System;

public sealed class AssetNumber : ValueObject<AssetNumber, string>, IValueObject<AssetNumber, string>, IComparable
{
    public static readonly AssetNumber Empty = new(string.Empty);
    
    // Required for Newtonsoft.Json deserialization
    private AssetNumber() { }

    private AssetNumber(string value) => Value = value;

    public static AssetNumber FromValue(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Empty;

        number = number.TrimStart(' ', 'A', 'C', '0');

        if (number.Length > 8)
            return Empty;

        if (!int.TryParse(number, out _))
            return Empty;

        AssetNumber assetNumber = new($"AC{number.PadLeft(8, '0')}");

        return assetNumber;
    }

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

    public override string ToString() => Value;

    public static implicit operator string(AssetNumber number) => number.ToString();

    public int CompareTo(object? obj)
    {
        if (obj is AssetNumber other)
            return string.Compare(Value, other.Value, StringComparison.Ordinal);

        return -1;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is AssetNumber other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Value);
    public static bool operator ==(AssetNumber left, AssetNumber right) => Equals(left, right);
    public static bool operator !=(AssetNumber left, AssetNumber right) => !Equals(left, right);

    public static bool operator <(AssetNumber? left, AssetNumber? right) => 
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(AssetNumber? left, AssetNumber? right) => 
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >(AssetNumber? left, AssetNumber? right) => 
        left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(AssetNumber? left, AssetNumber? right) => 
        left is not null && left.CompareTo(right) >= 0;
}
