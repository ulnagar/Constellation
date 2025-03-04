namespace Constellation.Application.Attendance.Plans.SubmitAttendancePlan;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record SubmitAttendancePlanCommand(
    AttendancePlanId PlanId)
    : ICommand;
