namespace Constellation.Core.Models.Subjects.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct CourseId(Guid Value)
    : IStronglyTypedId
{
    public static readonly CourseId Empty = new(Guid.Empty);

    public static CourseId FromValue(Guid value) =>
        new(value);

    public CourseId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
