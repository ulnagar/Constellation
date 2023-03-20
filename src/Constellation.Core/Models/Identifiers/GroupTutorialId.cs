namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record GroupTutorialId(Guid Value)
{
    public GroupTutorialId()
        : this(Guid.NewGuid()) { }
}
