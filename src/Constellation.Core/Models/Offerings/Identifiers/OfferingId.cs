namespace Constellation.Core.Models.Offerings.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct OfferingId(Guid Value)
    : IStronglyTypedId<OfferingId, Guid>
{
    public static OfferingId Empty => new(Guid.Empty);

    public static OfferingId FromValue(Guid value) =>
        new(value);

    public OfferingId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}
