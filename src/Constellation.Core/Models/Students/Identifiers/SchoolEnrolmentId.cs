using System;

namespace Constellation.Core.Models.Students.Identifiers;

public readonly record struct SchoolEnrolmentId(Guid Value)
{
    public static SchoolEnrolmentId Empty => new(Guid.Empty);

    public static SchoolEnrolmentId FromValue(Guid value) =>
        new(value);

    public SchoolEnrolmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}