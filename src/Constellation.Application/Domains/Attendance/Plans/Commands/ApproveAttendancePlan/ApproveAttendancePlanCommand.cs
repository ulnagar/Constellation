namespace Constellation.Application.Domains.Attendance.Plans.Commands.ApproveAttendancePlan;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record ApproveAttendancePlanCommand(
    AttendancePlanId PlanId,
    string Comment)
    : ICommand;