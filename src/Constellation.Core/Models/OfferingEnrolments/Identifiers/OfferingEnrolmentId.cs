namespace Constellation.Core.Models.OfferingEnrolments.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct OfferingEnrolmentId(Guid Value)
    : IStronglyTypedId
{
    public static OfferingEnrolmentId FromValue(Guid value) =>
        new(value);

    public OfferingEnrolmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}