namespace Constellation.Core.ValueObjects;

using Errors;
using Helpers;
using Newtonsoft.Json;
using Primitives;
using Shared;
using System.Text.RegularExpressions;

public sealed class PhoneNumber : ValueObject<PhoneNumber, string>, IValueObject<PhoneNumber, string>
{
    public static readonly PhoneNumber Empty = new(string.Empty);

    [JsonConstructor]
    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string? number)
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

        // Does it start with 61? Trim and add the 0
        if (trimmedNumber.Length == 11 && trimmedNumber[..2] == "61")
        {
            trimmedNumber = $"0{trimmedNumber[2..]}";
        }

        if (trimmedNumber.Length != 10)
        {
            return Result.Failure<PhoneNumber>(DomainErrors.ValueObjects.PhoneNumber.NumberInvalid);
        }

        if (!RegularExpressions.PhoneNumber().IsMatch(trimmedNumber))
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

        string prefix = Value[..2];

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
            Format.Mobile => $"{Value[..4]} {Value[4..7]} {Value[7..10]}",
            Format.LandLine => $"({Value[..2]}) {Value[2..6]} {Value[6..10]}",
            Format.International => $"61{Value[1..]}",
            _ => Value
        };
    
    public bool IsMobile()
    {
        if (this == Empty)
            return false;

        if (string.IsNullOrWhiteSpace(Value))
            return false;

        return Value[..2] switch
        {
            "04" => true,
            _ => false
        };
    }

    public enum Format
    {
        LandLine,
        Mobile,
        International,
        None
    }
}
