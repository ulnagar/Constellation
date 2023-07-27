namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record AwardNominationPeriodId(Guid Value)
{
    public static AwardNominationPeriodId FromValue(Guid Value) =>
        new(Value);

    public AwardNominationPeriodId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}