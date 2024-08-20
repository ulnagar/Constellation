namespace Constellation.Core.Models.Identifiers;

using System;

public record struct TutorialRollId(Guid Value)
{
    public static TutorialRollId Empty => new(Guid.Empty);

    public static TutorialRollId FromValue(Guid value) =>
        new(value);

    public TutorialRollId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
