namespace Constellation.Core.Models.StaffMembers.ValueObjects;

using Errors;
using Helpers;
using Primitives;
using Shared;
using System;
using System.ComponentModel;
using System.Globalization;

[TypeConverter(typeof(EmployeeIdConverter))]
public sealed class EmployeeId : ValueObject<EmployeeId, string>, IValueObject<EmployeeId, string>
{
    public static readonly EmployeeId Empty = new(string.Empty);

    private EmployeeId() { }

    private EmployeeId(string value)
    {
        Value = value;
    }

    public static Result<EmployeeId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<EmployeeId>(EmployeeIdErrors.EmptyValue);

        if (!RegularExpressions.EmployeeId().IsMatch(value))
            return Result.Failure<EmployeeId>(EmployeeIdErrors.InvalidValue(value));

        return new EmployeeId(value);
    }

    public override string ToString() => Value;

    /// <summary>
    /// Do not use. For EF Core only.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EmployeeId FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Empty;

        return new(value);
    }
}

public class EmployeeIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
            return EmployeeId.FromValue(stringValue);

        return base.ConvertFrom(context, culture, value);
    }
}