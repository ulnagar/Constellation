namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record ClassCoverId(Guid Value)
{
    public ClassCoverId()
        : this(Guid.NewGuid()) { }
}