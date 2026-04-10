namespace Constellation.Core.Models.Students.ValueObjects;

using Errors;
using Helpers;
using Primitives;
using Shared;

public sealed class StudentReferenceNumber : ValueObject<StudentReferenceNumber, string>, IValueObject<StudentReferenceNumber, string>
{
    public static readonly StudentReferenceNumber Empty = new(string.Empty);

    private StudentReferenceNumber() { }

    private StudentReferenceNumber(string value)
    {
        Value = value;
    }

    public static Result<StudentReferenceNumber> Create(string srn)
    {
        if (string.IsNullOrWhiteSpace(srn))
            return Result.Failure<StudentReferenceNumber>(StudentReferenceNumberErrors.EmptyValue);

        if (!RegularExpressions.StudentReferenceNumber().IsMatch(srn))
            return Result.Failure<StudentReferenceNumber>(StudentReferenceNumberErrors.InvalidValue(srn));

        return new StudentReferenceNumber(srn);
    }

    public override string ToString() => Value;

    /// <summary>
    /// Do not use. For EF Core only.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static StudentReferenceNumber FromValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Empty;

        return new(value);
    }

    public static implicit operator string(StudentReferenceNumber? studentReferenceNumber) =>
        studentReferenceNumber is null ? string.Empty : studentReferenceNumber.ToString();
}