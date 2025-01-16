namespace Constellation.Core.Models.Attendance.Identifiers;

using Primitives;
using System;

public readonly record struct AttendancePlanId(Guid Value)
    :IStronglyTypedId
{
    public static readonly AttendancePlanId Empty = new(Guid.Empty);

    public static AttendancePlanId FromValue(Guid value) =>
        new(value);

    public AttendancePlanId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}