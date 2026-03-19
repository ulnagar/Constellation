namespace Constellation.Core.ValueObjects;

using Errors;
using Newtonsoft.Json;
using Primitives;
using Shared;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class PhoneNumber : ValueObject
{
    public static readonly PhoneNumber Empty = new(string.Empty);

    private const string _regExFormat = @"^(?:\+?(61))? ?(?:\((?=.*\)))?(0?[2-57-8])\)? ?(\d\d(?:[- ](?=\d{3})|(?!\d\d[- ]?\d[- ]))\d\d[- ]?\d[- ]?\d{3})$";

    [JsonConstructor]
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

        string trimmedNumber = Regex.Replace(number, "[\\s+]", "");

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

    public static PhoneNumber FromValue(string number)
    {
        string trimmedNumber = Regex.Replace(number, "[\\s+]", "");

        return new(trimmedNumber);
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
            Format.International => $"61{Number[1..]}",
            Format.None => Number,
            _ => Number
        };
    
    private string Number { get; }

    public bool IsMobile()
    {
        if (this == Empty)
            return false;

        if (string.IsNullOrWhiteSpace(Number))
            return false;

        return Number[..2] switch
        {
            "04" => true,
            _ => false
        };
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
    }

    public enum Format
    {
        LandLine,
        Mobile,
        International,
        None
    }
}
