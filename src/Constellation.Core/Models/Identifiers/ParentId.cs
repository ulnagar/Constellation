namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record ParentId(Guid Value)
{
    public ParentId()
        : this(Guid.NewGuid()) { }
}
