namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record CasualId(Guid Value)
{
    public static CasualId FromValue(Guid value) =>
        new(value);

    public CasualId()
        : this(Guid.NewGuid()) { }
}
