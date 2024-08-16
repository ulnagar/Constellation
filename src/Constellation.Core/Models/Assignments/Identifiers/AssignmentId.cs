namespace Constellation.Core.Models.Assignments.Identifiers;

using System;

public record struct AssignmentId(Guid Value)
{
    public static AssignmentId Empty => new(Guid.Empty);
    public static AssignmentId FromValue(Guid value) =>
        new(value);

    public AssignmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
