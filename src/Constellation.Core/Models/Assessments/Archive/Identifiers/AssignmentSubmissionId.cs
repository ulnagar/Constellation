namespace Constellation.Core.Models.Assessments.Archive.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct AssignmentSubmissionId(Guid Value)
    : IStronglyTypedId<AssignmentSubmissionId, Guid>
{
    public static AssignmentSubmissionId Empty => new(Guid.Empty);

    public static AssignmentSubmissionId FromValue(Guid value) =>
        new(value);

    public AssignmentSubmissionId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}