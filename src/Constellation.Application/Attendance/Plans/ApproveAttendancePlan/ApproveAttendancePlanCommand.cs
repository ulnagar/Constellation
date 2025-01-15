namespace Constellation.Application.Attendance.Plans.ApproveAttendancePlan;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record ApproveAttendancePlanCommand(
    AttendancePlanId PlanId,
    string Comment)
    : ICommand;