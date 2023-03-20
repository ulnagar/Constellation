namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record CasualId(Guid Value)
{
    public CasualId()
        : this(Guid.NewGuid()) { }
}
