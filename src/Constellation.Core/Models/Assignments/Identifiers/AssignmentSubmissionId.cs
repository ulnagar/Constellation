namespace Constellation.Core.Models.Assignments.Identifiers;

using System;

public record struct AssignmentSubmissionId(Guid Value)
{
    public static AssignmentSubmissionId Empty => new(Guid.Empty);

    public static AssignmentSubmissionId FromValue(Guid value) =>
        new(value);

    public AssignmentSubmissionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}