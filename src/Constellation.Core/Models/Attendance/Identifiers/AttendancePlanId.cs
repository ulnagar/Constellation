namespace Constellation.Core.Models.Attendance.Identifiers;

using Primitives;
using System;

public readonly record struct AttendancePlanId(Guid Value)
    :IStronglyTypedId<AttendancePlanId, Guid>
{
    public static AttendancePlanId Empty => new(Guid.Empty);

    public static AttendancePlanId FromValue(Guid value) =>
        new(value);

    public AttendancePlanId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}