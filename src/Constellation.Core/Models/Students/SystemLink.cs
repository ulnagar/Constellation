namespace Constellation.Core.Models.Students;

using Enums;
using Errors;
using Identifiers;
using Shared;

public sealed class SystemLink
{
    private SystemLink(
        StudentId studentId,
        SystemType system,
        string value)
    {
        StudentId = studentId;
        System = system;
        Value = value;
    }

    public StudentId StudentId { get; private set; }
    public SystemType System { get; private set; }
    public string Value { get; private set; }

    internal static Result<SystemLink> Create(
        StudentId studentId,
        SystemType system,
        string value)
    {
        if (studentId == StudentId.Empty)
            return Result.Failure<SystemLink>(StudentErrors.InvalidId);

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<SystemLink>(SystemLinkErrors.EmptyValue);

        return new SystemLink(
            studentId,
            system,
            value);
    }
}