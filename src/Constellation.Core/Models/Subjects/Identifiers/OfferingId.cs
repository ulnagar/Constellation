namespace Constellation.Core.Models.Subjects.Identifiers;

using System;

public sealed record CourseId(Guid Value)
{
    public static readonly CourseId Empty = new(Guid.Empty);

    public static CourseId FromValue(Guid value) =>
        new(value);

    public CourseId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
