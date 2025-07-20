namespace Constellation.Core.Models.Tutorials.Identifiers;

using Primitives;
using System;

public readonly record struct TutorialSessionId(Guid Value)
    : IStronglyTypedId
{
    public static readonly TutorialSessionId Empty = new(Guid.Empty);

    public static TutorialSessionId FromValue(Guid value) =>
        new(value);

    public TutorialSessionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}