namespace Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct OfferId(Guid Value)
    : IStronglyTypedId<OfferId, Guid>
{
    public static OfferId Empty => new(Guid.Empty);

    public static OfferId FromValue(Guid value) =>
        new(value);

    public OfferId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();

}