namespace Constellation.Core.Models.Faculties.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct FacultyId(Guid Value)
    : IStronglyTypedId
{
    public static FacultyId Empty => new(Guid.Empty);

    public static FacultyId FromValue(Guid value) =>
        new(value);

    public FacultyId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}