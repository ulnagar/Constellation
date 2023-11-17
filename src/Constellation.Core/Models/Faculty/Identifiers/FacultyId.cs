namespace Constellation.Core.Models.Faculty.Identifiers;

using System;

public sealed record FacultyId(Guid Value)
{
    public static FacultyId FromValue(Guid value) =>
        new(value);

    public FacultyId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}