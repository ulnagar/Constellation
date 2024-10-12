namespace Constellation.Core.Models.Faculties.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct FacultyMembershipId(Guid Value)
    : IStronglyTypedId
{
    public static FacultyMembershipId Empty => new(Guid.Empty);

    public static FacultyMembershipId FromValue(Guid value) =>
        new(value);

    public FacultyMembershipId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}