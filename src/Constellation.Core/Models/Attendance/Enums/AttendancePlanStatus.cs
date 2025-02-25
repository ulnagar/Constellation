namespace Constellation.Core.Models.Attendance.Enums;

using Common;
using System.Collections.Generic;

public sealed class AttendancePlanStatus : StringEnumeration<AttendancePlanStatus>
{
    public static readonly AttendancePlanStatus Pending = new("Pending", 1);
    public static readonly AttendancePlanStatus Processing = new("Processing", 2);
    public static readonly AttendancePlanStatus Accepted = new("Accepted", 3);
    public static readonly AttendancePlanStatus Rejected = new("Rejected", 4);
    public static readonly AttendancePlanStatus Superseded = new("Superseded", 5);
    public static readonly AttendancePlanStatus Archived = new("Archived", 6);

    private AttendancePlanStatus(string value, int order)
        : base(value, value, order) { }

    public static IEnumerable<AttendancePlanStatus> GetOptions => GetEnumerable;
}