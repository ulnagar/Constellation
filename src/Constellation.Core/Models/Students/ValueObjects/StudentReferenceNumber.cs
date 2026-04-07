namespace Constellation.Core.Models.Students.ValueObjects;

using Errors;
using Helpers;
using Primitives;
using Shared;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class StudentReferenceNumber : ValueObject
{
    public static readonly StudentReferenceNumber Empty = new(string.Empty);

    public string Number { get; }

    private StudentReferenceNumber() { }

    private StudentReferenceNumber(string number)
    {
        Number = number;
    }

    public static Result<StudentReferenceNumber> Create(string srn)
    {
        if (string.IsNullOrWhiteSpace(srn))
            return Result.Failure<StudentReferenceNumber>(StudentReferenceNumberErrors.EmptyValue);

        if (!RegularExpressions.StudentReferenceNumber().IsMatch(srn))
            return Result.Failure<StudentReferenceNumber>(StudentReferenceNumberErrors.InvalidValue(srn));

        return new StudentReferenceNumber(srn);
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
    public static StudentReferenceNumber FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Empty;

        return new(value);
    }
}