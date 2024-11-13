namespace Constellation.Core.Models.Canvas.Models;

using Offerings;
using System;

public readonly record struct CanvasCourseCode : IComparable
{
    public static CanvasCourseCode Empty => new(string.Empty);

    private readonly string _value;

    private CanvasCourseCode(string value) : this() => _value = value;
    
    public static CanvasCourseCode FromOffering(Offering offering)
    {
        string year = offering.EndDate.Year.ToString();

        return new($"{year}-{offering.Name.Value[..^2]}");
    }

    public static CanvasCourseCode FromValue(string value)
    {
        if (value.Length > 10)
            return new(value[..10]);

        return new(value);
    }

    public override string ToString() => _value;

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;

        CanvasCourseCode other = (CanvasCourseCode)obj;

        return string.Compare(_value, other._value, StringComparison.Ordinal);
    }

    public static bool operator <(CanvasCourseCode left, CanvasCourseCode right) => left.CompareTo(right) < 0;

    public static bool operator <=(CanvasCourseCode left, CanvasCourseCode right) => left.CompareTo(right) <= 0;

    public static bool operator >(CanvasCourseCode left, CanvasCourseCode right) => left.CompareTo(right) > 0;

    public static bool operator >=(CanvasCourseCode left, CanvasCourseCode right) => left.CompareTo(right) >= 0;
}