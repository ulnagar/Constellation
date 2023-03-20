namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record FamilyId(Guid Value)
{
    public FamilyId()
        : this(Guid.NewGuid()) { }
}