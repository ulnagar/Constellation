using System;

namespace Constellation.Core.Models.Grade.Identifiers;

public readonly record struct CohortId(Guid Value)
{
    public static CohortId Empty => new(Guid.Empty);

    public static CohortId FromValue(Guid value) =>
        new(value);

    public CohortId()
        : this(Guid.NewGuid()) { }

    public override string ToString() => 
        Value.ToString();
}