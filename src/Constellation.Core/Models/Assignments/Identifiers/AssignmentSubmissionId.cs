namespace Constellation.Core.Models.Assignments.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct AssignmentSubmissionId(Guid Value)
    : IStronglyTypedId
{
    public static AssignmentSubmissionId Empty => new(Guid.Empty);

    public static AssignmentSubmissionId FromValue(Guid value) =>
        new(value);

    public AssignmentSubmissionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}