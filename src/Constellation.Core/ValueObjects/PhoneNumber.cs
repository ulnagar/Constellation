namespace Constellation.Core.ValueObjects;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class PhoneNumber : ValueObject
{
    private const string _regExFormat = @"^(?:\+?(61))? ?(?:\((?=.*\)))?(0?[2-57-8])\)? ?(\d\d(?:[- ](?=\d{3})|(?!\d\d[- ]?\d[- ]))\d\d[- ]?\d[- ]?\d{3})$";

    private PhoneNumber(string number)
    {
        Number = number;
    }

    public static Result<PhoneNumber> Create(string number)
    {
        var trimmedNumber = Regex.Replace(number, "[^0-9]", "");

        if (trimmedNumber.Length == 8)
        {
            trimmedNumber = $"02{trimmedNumber}";
        }

        if (trimmedNumber.Length != 10)
        {
            return Result.Failure<PhoneNumber>(DomainErrors.ValueObjects.PhoneNumber.NumberInvalid);
        }

        if (string.IsNullOrWhiteSpace(trimmedNumber))
        {
            return Result.Failure<PhoneNumber>(DomainErrors.ValueObjects.PhoneNumber.NumberEmpty);
        }

        if (!Regex.IsMatch(trimmedNumber, _regExFormat))
        {
            return Result.Failure<PhoneNumber>(DomainErrors.ValueObjects.PhoneNumber.NumberInvalid);
        }

        return new PhoneNumber(trimmedNumber);
    }

    public override string ToString()
    {
        var prefix = Number[..2];

        if (prefix == "04" ||
            prefix == "13")
            return ToString(Format.Mobile);

        if (prefix == "02" || 
            prefix == "03" ||
            prefix == "07" ||
            prefix == "08")
            return ToString(Format.LandLine);

        return ToString(Format.None);
    }

    public string ToString(Format format)
    {
        if (format == Format.Mobile)
        {
            return $"{Number[..4]} {Number[5..7]} {Number[8..10]}";
        }

        if (format == Format.LandLine)
        {
            return $"({Number[..2]}) {Number[3..6]} {Number[7..10]}";
        }

        return Number;
    }

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
