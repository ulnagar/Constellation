using System;

namespace Constellation.Core.Models.Students.Identifiers;

public readonly record struct SchoolEnrollmentId(Guid Value)
{
    public static SchoolEnrollmentId Empty => new(Guid.Empty);

    public static SchoolEnrollmentId FromValue(Guid value) =>
        new(value);

    public SchoolEnrollmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}