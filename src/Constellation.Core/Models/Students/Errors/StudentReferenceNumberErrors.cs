using Constellation.Core.Shared;
using System;

namespace Constellation.Core.Models.Students.Errors;

public static class StudentReferenceNumberErrors
{
    public static readonly Error EmptyValue = new(
        "Student.ReferenceNumber.Empty",
        "An SRN must have a value");

    public static readonly Func<string, Error> InvalidValue = id => new(
        "Student.ReferenceNumber.Invalid",
        $"The provided SRN '{id}' is invalid");
}