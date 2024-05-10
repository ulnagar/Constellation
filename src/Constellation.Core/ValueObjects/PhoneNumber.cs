namespace Constellation.Core.ValueObjects;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class PhoneNumber : ValueObject
{
    public static readonly PhoneNumber Empty = new("0200000000");

    private const string _regExFormat = @"^(?:\+?(61))? ?(?:\((?=.*\)))?(0?[2-57-8])\)? ?(\d\d(?:[- ](?=\d{3})|(?!\d\d[- ]?\d[- ]))\d\d[- ]?\d[- ]?\d{3})$";

    private PhoneNumber(string number)
    {
        Number = number;
    }

    public static Result<PhoneNumber> Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return Result.Failure<PhoneNumber>(DomainErrors.ValueObjects.PhoneNumber.NumberEmpty);
        }

        var trimmedNumber = Regex.Replace(number, "[^0-9]", "");

        if (trimmedNumber.Length == 8)
        {
            trimmedNumber = $"02{trimmedNumber}";
        }

        if (trimmedNumber.Length != 10)
        {
            return Result.Failure<PhoneNumber>(DomainErrors.ValueObjects.PhoneNumber.NumberInvalid);
        }

        if (!Regex.IsMatch(trimmedNumber, _regExFormat))
        {
            return Result.Failure<PhoneNumber>(DomainErrors.ValueObjects.PhoneNumber.NumberInvalid);
        }

        return new PhoneNumber(trimmedNumber);
    }

    public override string ToString()
    {
        if (this == Empty)
            return string.Empty;

        string prefix = Number[..2];

        switch (prefix)
        {
            case "04":
            case "13":
                return ToString(Format.Mobile);
            case "02":
            case "03":
            case "07":
            case "08":
                return ToString(Format.LandLine);
            default:
                return ToString(Format.None);
        }
    }

    public string ToString(Format format) =>
        format switch
        {
            Format.Mobile => $"{Number[..4]} {Number[4..7]} {Number[7..10]}",
            Format.LandLine => $"({Number[..2]}) {Number[2..6]} {Number[6..10]}",
            Format.None => Number,
            _ => Number
        };

    private string Number { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
    }

    public enum Format
    {
        LandLine,
        Mobile,
        None
    }
}
