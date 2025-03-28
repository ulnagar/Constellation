namespace Constellation.Core.Models.Prospects.ValueObjects;

using Constellation.Core.Primitives;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class ApplicationNumber : ValueObject
{
    private const string _regex = @"^[CS]{1}\d{8}$";

    public static readonly ApplicationNumber Empty = new(string.Empty);

    public string Number { get; }

    private ApplicationNumber() { }

    private ApplicationNumber(string number)
    {
        Number = number;
    }

    public static ApplicationNumber Create(string applicationNumber)
    {
        if (string.IsNullOrWhiteSpace(applicationNumber))
            return Empty;

        if (!Regex.IsMatch(applicationNumber, _regex))
            return Empty;

        return new(applicationNumber);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
    }

    public override string ToString() => Number;

    /// <summary>
    /// Do not use. For EF Core only.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ApplicationNumber FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Empty;

        return new(value);
    }
}