namespace Constellation.Core.Enums;

using Common;
using Errors;
using Shared;
using System.ComponentModel;

[TypeConverter(typeof(GradeTypeConverter))]
public sealed class Grade : StringEnumeration<Grade>
{
    public static readonly Grade Empty = new("", "", 0);

    public static readonly Grade Y05 = new("Y05", "Year 5", 5);
    public static readonly Grade Y06 = new("Y06", "Year 6", 6);
    public static readonly Grade Y07 = new("Y07", "Year 7", 7);
    public static readonly Grade Y08 = new("Y08", "Year 8", 8);
    public static readonly Grade Y09 = new("Y09", "Year 9", 9);
    public static readonly Grade Y10 = new("Y10", "Year 10", 10);
    public static readonly Grade Y11 = new("Y11", "Year 11", 11);
    public static readonly Grade Y12 = new("Y12", "Year 12", 12);
    
    private Grade(string value, string name, int order)
        : base(value, name, order)
    { }

    public static IEnumerable<Grade> GetOptions => GetEnumerable;

    public static Result<Grade> FromNumber(int number)
    {
        Grade? grade = GetEnumerable
            .Where(entry => entry.Order > 0)
            .FirstOrDefault(entry => entry.Order == number);

        if (grade is not null)
            return grade;

        return Result.Failure<Grade>(GradeErrors.NotFound(number));
    }

    public static bool TryParse(string? input, out Grade grade)
    {
        grade = Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Numeric string input matches against Order
        if (int.TryParse(input, out int number))
            return TryParse(number, out grade);

        grade = GetEnumerable.FirstOrDefault(entry =>
            string.Equals(entry.Value, input, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(entry.Name, input, StringComparison.OrdinalIgnoreCase)) ?? Empty;

        return grade != Empty;
    }

    public static bool TryParse(int input, out Grade grade)
    {
        grade = GetEnumerable.FirstOrDefault(entry =>
            entry.Order > 0 && entry.Order == input) ?? Empty;

        return grade != Empty;
    }

    public Result<Grade> Next() => 
        FromNumber(Order + 1);
}

public sealed class GradeTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue)) return Grade.Empty;

            return Grade.TryParse(stringValue, out Grade grade)
                ? grade
                : throw new FormatException($"Invalid grade value: {stringValue}");
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType) =>
            destinationType == typeof(string) && value is Grade grade
            ? grade.Value
            : base.ConvertTo(context, culture, value, destinationType);
}