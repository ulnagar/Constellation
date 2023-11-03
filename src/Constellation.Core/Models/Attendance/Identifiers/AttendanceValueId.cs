namespace Constellation.Core.Models.Attendance.Identifiers;

using System;

public sealed record AttendanceValueId(Guid Value)
{
    public static AttendanceValueId FromValue(Guid value) =>
        new(value);

    public AttendanceValueId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}