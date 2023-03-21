namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TutorialTeacherId(Guid Value)
{
    public static TutorialTeacherId FromValue(Guid value) =>
        new(value);

    public TutorialTeacherId()
        : this(Guid.NewGuid()) { }
}
