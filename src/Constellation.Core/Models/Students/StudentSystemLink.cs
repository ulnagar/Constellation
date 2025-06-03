namespace Constellation.Core.Models.Students;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Core.Enums;
using Shared;
using Students.Errors;
using Students.Identifiers;

public sealed class StudentSystemLink : SystemLink
{
    private StudentSystemLink(
        StudentId studentId,
        SystemType system,
        string value)
        : base(system, value)
    {
        StudentId = studentId;
    }

    public StudentId StudentId { get; private set; }

    internal static Result<StudentSystemLink> Create(
        StudentId studentId,
        SystemType system,
        string value)
    {
        if (studentId == StudentId.Empty)
            return Result.Failure<StudentSystemLink>(StudentErrors.InvalidId);

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<StudentSystemLink>(SystemLinkErrors.EmptyValue);

        return new StudentSystemLink(
            studentId,
            system,
            value);
    }
}