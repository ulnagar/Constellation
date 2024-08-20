namespace Constellation.Core.Models.Identifiers;

using System;

public record struct TutorialTeacherId(Guid Value)
{
    public static TutorialTeacherId Empty => new(Guid.Empty);

    public static TutorialTeacherId FromValue(Guid value) =>
        new(value);

    public TutorialTeacherId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
