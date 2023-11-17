namespace Constellation.Core.Models.Faculty.Identifiers;

using System;

public sealed record FacultyMembershipId(Guid Value)
{
    public static FacultyMembershipId FromValue(Guid value) =>
        new(value);

    public FacultyMembershipId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}