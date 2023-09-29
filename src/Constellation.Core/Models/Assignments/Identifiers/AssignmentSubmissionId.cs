namespace Constellation.Core.Models.Assignments.Identifiers;

using System;

public sealed record AssignmentSubmissionId(Guid Value)
{
    public static AssignmentSubmissionId FromValue(Guid Value) =>
        new(Value);

    public AssignmentSubmissionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}