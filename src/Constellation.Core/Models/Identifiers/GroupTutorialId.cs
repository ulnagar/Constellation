namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct GroupTutorialId(Guid Value)
    : IStronglyTypedId
{
    public static GroupTutorialId Empty => new(Guid.Empty);

    public GroupTutorialId()
        : this(Guid.NewGuid()) { }

    public static GroupTutorialId FromValue(Guid value) =>
        new(value);

    public override string ToString() =>
        Value.ToString();
}
