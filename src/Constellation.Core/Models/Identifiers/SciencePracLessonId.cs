namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record SciencePracLessonId(Guid Value)
{
    public static SciencePracLessonId FromValue(Guid Value) =>
        new(Value);

    public SciencePracLessonId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
