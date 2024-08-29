namespace Constellation.Core.Models.Students.Errors;

using Shared;
using System;

public static class StudentReferenceNumberErrors
{
    public static readonly Error EmptyValue = new(
        "Student.ReferenceNumber.Empty",
        "An SRN must have a value");

    public static readonly Func<string, Error> InvalidValue = id => new(
        "Student.ReferenceNumber.Invalid",
        $"The provided SRN '{id}' is invalid");
}