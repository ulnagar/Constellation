using System.Runtime.CompilerServices;

namespace Constellation.Core.Models.Students.ValueObjects;

using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class StudentReferenceNumber : ValueObject
{
    private const string _srnRegex = @"^\d{10}$";

    public static readonly StudentReferenceNumber Empty = new(string.Empty);

    public string Number { get; }

    private StudentReferenceNumber(string srn)
    {
        Number = srn;
    }

    public static Result<StudentReferenceNumber> Create(string srn)
    {
        if (string.IsNullOrWhiteSpace(srn))
            return Result.Failure<StudentReferenceNumber>(StudentReferenceNumberErrors.EmptyValue);

        if (!Regex.IsMatch(srn, _srnRegex))
            return Result.Failure<StudentReferenceNumber>(StudentReferenceNumberErrors.InvalidValue(srn));

        return new StudentReferenceNumber(srn);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
    }

    public override string ToString() => Number;
}