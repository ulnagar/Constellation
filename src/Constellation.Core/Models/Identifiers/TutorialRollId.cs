namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TutorialRollId(Guid Value)
{
    public static TutorialRollId FromValue(Guid value) =>
        new(value);

    public TutorialRollId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
