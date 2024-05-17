namespace Constellation.Core.Models.Attendance.Identifiers;

using System;

public readonly record struct AttendanceValueId(Guid Value)
{
    public static readonly AttendanceValueId Empty = new (Guid.Empty);

    public static AttendanceValueId FromValue(Guid value) =>
        new(value);

    public AttendanceValueId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}