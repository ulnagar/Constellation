namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct TutorialRollId(Guid Value)
    : IStronglyTypedId<TutorialRollId, Guid>
{
    public static TutorialRollId Empty => new(Guid.Empty);

    public static TutorialRollId FromValue(Guid value) =>
        new(value);

    public TutorialRollId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}
