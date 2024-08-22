namespace Constellation.Core.Models.Offerings.Identifiers;

using System;

public readonly record struct AssignmentId(Guid Value)
{
    public static AssignmentId Empty => new(Guid.Empty);

    public static AssignmentId FromValue(Guid value) =>
        new(value);

    public AssignmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}