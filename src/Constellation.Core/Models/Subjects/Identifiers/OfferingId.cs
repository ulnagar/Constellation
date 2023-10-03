namespace Constellation.Core.Models.Subjects.Identifiers;

using System;

public sealed record CourseId(Guid Value)
{
    public static CourseId FromValue(Guid Value) =>
        new(Value);

    public CourseId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
