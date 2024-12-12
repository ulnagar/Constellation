namespace Constellation.Core.Models.Attendance.Enums;

using Common;

public sealed class AttendancePlanStatus : StringEnumeration<AttendancePlanStatus>
{
    public static readonly AttendancePlanStatus Pending = new("Pending");
    public static readonly AttendancePlanStatus Processing = new("Processing");
    public static readonly AttendancePlanStatus Accepted = new("Accepted");
    public static readonly AttendancePlanStatus Rejected = new("Rejected");
    public static readonly AttendancePlanStatus Superseded = new("Superseded");

    private AttendancePlanStatus(string value)
        : base(value, value) { }
}