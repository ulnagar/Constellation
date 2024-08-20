namespace Constellation.Core.Models.Faculties.Identifiers;

using System;

public record struct FacultyId(Guid Value)
{
    public static FacultyId Empty => new(Guid.Empty);

    public static FacultyId FromValue(Guid value) =>
        new(value);

    public FacultyId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}