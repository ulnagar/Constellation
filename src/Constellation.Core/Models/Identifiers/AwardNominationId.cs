namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record AwardNominationId(Guid Value)
    : IStronglyTypedId
{
    public static AwardNominationId FromValue(Guid Value) =>
        new(Value);

    public AwardNominationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}