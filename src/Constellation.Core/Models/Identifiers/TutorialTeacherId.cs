namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TutorialTeacherId(Guid Value)
{
    public TutorialTeacherId()
        : this(Guid.NewGuid()) { }
}
