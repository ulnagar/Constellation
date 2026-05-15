namespace Constellation.Core.Models.Tutorials.Identifiers;

using Primitives;
using System;

public readonly record struct TutorialSessionId(Guid Value)
    : IStronglyTypedId<TutorialSessionId, Guid>
{
    public static TutorialSessionId Empty => new(Guid.Empty);

    public static TutorialSessionId FromValue(Guid value) =>
        new(value);

    public TutorialSessionId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}