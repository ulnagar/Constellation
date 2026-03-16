namespace Constellation.Core.Models.Students.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SchoolEnrolmentId(Guid Value)
    : IStronglyTypedId
{
    public static SchoolEnrolmentId Empty => new(Guid.Empty);

    public static SchoolEnrolmentId FromValue(Guid value) =>
        new(value);

    public SchoolEnrolmentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}