namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record AwardNominationId(Guid Value)
{
    public static AwardNominationId FromValue(Guid Value) =>
        new(Value);

    public AwardNominationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}