namespace Constellation.Core.Models.Students.Identifiers;

using Primitives;
using System;

public readonly record struct StudentId(Guid Value)
    : IStronglyTypedId
{
    public static StudentId Empty => new(Guid.Empty);

    public static StudentId FromValue(Guid value) =>
        new(value);

    public StudentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}