namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct AwardNominationPeriodId(Guid Value)
    : IStronglyTypedId
{
    public static AwardNominationPeriodId Empty => new(Guid.Empty);

    public static AwardNominationPeriodId FromValue(Guid value) =>
        new(value);

    public AwardNominationPeriodId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}