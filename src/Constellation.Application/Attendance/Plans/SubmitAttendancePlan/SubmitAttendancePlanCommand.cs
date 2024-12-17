namespace Constellation.Application.Attendance.Plans.SubmitAttendancePlan;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;
using System;
using System.Collections.Generic;

public sealed record SubmitAttendancePlanCommand(
    AttendancePlanId PlanId,
    List<SubmitAttendancePlanCommand.PlanPeriod> Periods)
: ICommand
{
    public sealed record PlanPeriod(
        AttendancePlanPeriodId PlanPeriodId,
        TimeOnly EntryTime,
        TimeOnly ExitTime);
}
