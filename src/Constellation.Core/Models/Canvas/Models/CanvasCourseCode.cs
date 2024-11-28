namespace Constellation.Core.Models.Canvas.Models;

using System;

public readonly record struct CanvasCourseCode : IComparable
{
    public static CanvasCourseCode Empty => new(string.Empty);

    private readonly string _value;

    private CanvasCourseCode(string value) : this() => _value = value;
    
    public static CanvasCourseCode FromValue(string value) => new(value);

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