namespace Constellation.Core.Models.Attendance.Identifiers;

using Primitives;
using System;

public readonly record struct AttendancePlanPeriodId(Guid Value)
    : IStronglyTypedId
{
    public static readonly AttendancePlanPeriodId Empty = new(Guid.Empty);

    public static AttendancePlanPeriodId FromValue(Guid value) =>
        new(value);

    public AttendancePlanPeriodId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}