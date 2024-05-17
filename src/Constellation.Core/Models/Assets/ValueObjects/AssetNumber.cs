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
            return Result.Failure<AssetNumber>(AssetErrors.AssetNumber.Empty);

        number = number.TrimStart(' ', 'A', 'C', '0');

        if (number.Length > 8)
            return Result.Failure<AssetNumber>(AssetErrors.AssetNumber.TooLong);

        if (!int.TryParse(number, out int intNumber))
            return Result.Failure<AssetNumber>(AssetErrors.AssetNumber.UnknownCharacters);

        AssetNumber assetNumber = new($"AC{number.PadLeft(8, '0')}");

        return assetNumber;
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
    }

    public int CompareTo(object obj)
    {
        if (obj is AssetNumber other)
            return string.Compare(Number, other.Number, StringComparison.Ordinal);

        return -1;
    }
}
