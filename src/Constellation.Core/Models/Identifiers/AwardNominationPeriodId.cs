namespace Constellation.Core.Models.Identifiers;

using System;

public record struct AwardNominationPeriodId(Guid Value)
{
    public static AwardNominationPeriodId Empty => new(Guid.Empty);

    public static AwardNominationPeriodId FromValue(Guid value) =>
        new(value);

    public AwardNominationPeriodId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}