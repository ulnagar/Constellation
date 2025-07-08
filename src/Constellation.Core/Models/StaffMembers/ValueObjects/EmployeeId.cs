namespace Constellation.Core.Models.StaffMembers.ValueObjects;

using Errors;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

[TypeConverter(typeof(EmployeeIdConverter))]
public sealed class EmployeeId : ValueObject
{
    private const string _srnRegex = @"^\d{6,9}$";

    public static readonly EmployeeId Empty = new(string.Empty);

    public string Number { get; }

    private EmployeeId() { }

    private EmployeeId(string number)
    {
        Number = number;
    }

    public static Result<EmployeeId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<EmployeeId>(EmployeeIdErrors.EmptyValue);

        if (!Regex.IsMatch(value, _srnRegex))
            return Result.Failure<EmployeeId>(EmployeeIdErrors.InvalidValue(value));

        return new EmployeeId(value);
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