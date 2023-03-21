namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record ParentId(Guid Value)
{
    public static ParentId FromValue(Guid value) =>
        new(value);

    public ParentId()
        : this(Guid.NewGuid()) { }
}
