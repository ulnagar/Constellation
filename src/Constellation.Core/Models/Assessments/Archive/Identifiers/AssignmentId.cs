namespace Constellation.Core.Models.Assessments.Archive.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct AssignmentId(Guid Value)
    : IStronglyTypedId<AssignmentId, Guid>
{
    public static AssignmentId Empty => new(Guid.Empty);
    public static AssignmentId FromValue(Guid value) =>
        new(value);

    public AssignmentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}