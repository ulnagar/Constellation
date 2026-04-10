namespace Constellation.Core.Models.Tutorials.Identifiers;

using Primitives;
using System;

public readonly record struct TutorialId(Guid Value)
    :IStronglyTypedId<TutorialId, Guid>
{
    public static readonly TutorialId Empty = new(Guid.Empty);

    public static TutorialId FromValue(Guid value) =>
        new(value);

    public TutorialId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}