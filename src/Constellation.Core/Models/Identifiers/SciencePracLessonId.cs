namespace Constellation.Core.Models.Identifiers;

using System;

public readonly record struct SciencePracLessonId(Guid Value)
{
    public static SciencePracLessonId Empty => new(Guid.Empty);

    public static SciencePracLessonId FromValue(Guid value) =>
        new(value);

    public SciencePracLessonId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
