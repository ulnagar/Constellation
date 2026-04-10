namespace Constellation.Core.Models.Awards.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AwardNominationId(Guid Value)
    : IStronglyTypedId<AwardNominationId, Guid>
{
    public static AwardNominationId FromValue(Guid Value) =>
        new(Value);

    public AwardNominationId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}