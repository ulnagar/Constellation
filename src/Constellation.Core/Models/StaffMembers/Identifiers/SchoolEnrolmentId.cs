namespace Constellation.Core.Models.StaffMembers.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SchoolAssignmentId(Guid Value)
    : IStronglyTypedId
{
    public static SchoolAssignmentId Empty => new(Guid.Empty);

    public static SchoolAssignmentId FromValue(Guid value) =>
        new(value);

    public SchoolAssignmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}