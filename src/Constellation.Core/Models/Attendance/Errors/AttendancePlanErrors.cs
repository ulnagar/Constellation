namespace Constellation.Core.Models.Attendance.Errors;

using Enums;
using Shared;
using System;

public static class AttendancePlanErrors
{
    public static readonly Func<AttendancePlanStatus, Error> InvalidCurrentStatus = currentStatus => new(
        "AttendancePlans.UpdateStatus.InvalidCurrentStatus",
        $"Cannot update status when the current status is {currentStatus.Name}");

    public static readonly Func<AttendancePlanStatus, Error> InvalidNewStatus = newStatus => new(
        "AttendancePlans.UpdateStatus.InvalidNewStatus",
        $"Cannot update status to {newStatus.Name}");
}