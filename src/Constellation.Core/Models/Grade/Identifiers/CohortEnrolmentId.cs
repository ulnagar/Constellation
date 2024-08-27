using System;

namespace Constellation.Core.Models.Grade.Identifiers;

public readonly record struct CohortEnrolmentId(Guid Value)
{
    public static CohortEnrolmentId Empty => new(Guid.Empty);

    public static CohortEnrolmentId FromValue(Guid value) =>
        new(value);

    public CohortEnrolmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}