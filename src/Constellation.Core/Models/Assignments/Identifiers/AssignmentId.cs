namespace Constellation.Core.Models.Assignments.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct AssignmentId(Guid Value)
    : IStronglyTypedId
{
    public static AssignmentId Empty => new(Guid.Empty);
    public static AssignmentId FromValue(Guid value) =>
        new(value);

    public AssignmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
