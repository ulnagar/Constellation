namespace Constellation.Core.Models.Assignments.Identifiers;

using System;

public sealed record AssignmentId(Guid Value)
{
    public static AssignmentId FromValue(Guid Value) =>
        new(Value);

    public AssignmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
