namespace Constellation.Core.Models.Attendance.Errors;

using Enums;
using Identifiers;
using Shared;
using System;

public static class AttendancePlanErrors
{
    public static readonly Func<AttendancePlanId, Error> NotFound = id => new(
        "AttendancePlan.NotFound",
        $"Could not find an Attendance Plan with the id {id}");

    public static readonly Func<AttendancePlanStatus, Error> InvalidCurrentStatus = currentStatus => new(
        "AttendancePlans.UpdateStatus.InvalidCurrentStatus",
        $"Cannot update status when the current status is {currentStatus.Name}");

    public static readonly Func<AttendancePlanStatus, Error> InvalidNewStatus = newStatus => new(
        "AttendancePlans.UpdateStatus.InvalidNewStatus",
        $"Cannot update status to {newStatus.Name}");

    public static readonly Func<AttendancePlanPeriodId, Error> PeriodNotFound = periodId => new(
        "AttendancePlans.UpdatePeriods.PeriodNotFound",
        $"A Period with the Id {periodId} could not be found");

    public static readonly Error EntryTimeBeforeStartTime = new(
        "AttendancePlans.UpdatePeriods.EntryTimeBeforeStartTime",
        "The supplied entry time is before the start of the period");

    public static readonly Error EntryTimeAfterEndTime = new(
        "AttendancePlans.UpdatePeriods.EntryTimeAfterEndTime",
        "The supplied entry time is after the end of the period");

    public static readonly Error ExitTimeBeforeStartTime = new(
        "AttendancePlans.UpdatePeriods.ExitTimeBeforeStartTime",
        "The supplied exit time is before the start of the period");

    public static readonly Error ExitTimeAfterEndTime = new(
        "AttendancePlans.UpdatePeriods.ExitTimeAfterEndTime",
        "The supplied exit time is after the end of the period");

    public static readonly Error EntryTimeAfterExitTime = new(
        "AttendancePlans.UpdatePeriods.EntryTimeAfterExitTime",
        "The supplied entry time is after the supplied exit time");

    public static readonly Error CommentRequired = new(
        "AttendancePlans.UpdateStatus.CommentRequired",
        "A comment is required to update the plan status");

    public static readonly Error StatusNotAccepted = new(
        "AttendancePlans.StatusNotAccepted",
        "The Attendance Plan must be Accepted to process this action");

    public static readonly Error NoPeriodsFound = new(
        "AttendancePlans.NoPeriodsFound",
        "No suitable periods found when attempting to generate the Attendance Plan");

    public static readonly Error ExistingInProgressPlanFound = new(
        "AttendancePlans.ExistingInProgressPlanFound",
        "An existing Attendance Plan with a status of either Pending or Processing exists for this student");
}