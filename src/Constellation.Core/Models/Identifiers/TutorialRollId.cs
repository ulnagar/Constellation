namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TutorialRollId(Guid Value)
{
    public TutorialRollId()
        : this(Guid.NewGuid()) { }
}
