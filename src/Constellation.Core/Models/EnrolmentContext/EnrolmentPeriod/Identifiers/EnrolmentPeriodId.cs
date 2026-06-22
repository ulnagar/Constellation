namespace Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;

using Primitives;
using System;

public readonly record struct EnrolmentPeriodId(Guid Value)
    : IStronglyTypedId<EnrolmentPeriodId, Guid>
{
    public static EnrolmentPeriodId Empty => new(Guid.Empty);

    public static EnrolmentPeriodId FromValue(Guid value) =>
        new(value);

    public EnrolmentPeriodId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();

}
