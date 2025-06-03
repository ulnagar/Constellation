namespace Constellation.Core.Models.StaffMembers.ValueObjects;

using Errors;
using Primitives;
using Shared;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class EmployeeId : ValueObject
{
    private const string _srnRegex = @"^\d{9}$";

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

        return new EmployeeId();
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