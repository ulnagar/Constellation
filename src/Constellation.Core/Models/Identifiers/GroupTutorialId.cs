namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct GroupTutorialId(Guid Value)
    : IStronglyTypedId<GroupTutorialId, Guid>
{
    public static GroupTutorialId Empty => new(Guid.Empty);

    public GroupTutorialId()
        : this(Guid.CreateVersion7()) { }

    public static GroupTutorialId FromValue(Guid value) =>
        new(value);

    public override string ToString() =>
        Value.ToString();
}
