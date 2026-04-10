namespace Constellation.Core.Models.Awards.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct AwardNominationPeriodId(Guid Value)
    : IStronglyTypedId<AwardNominationPeriodId, Guid>
{
    public static AwardNominationPeriodId Empty => new(Guid.Empty);

    public static AwardNominationPeriodId FromValue(Guid value) =>
        new(value);

    public AwardNominationPeriodId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}