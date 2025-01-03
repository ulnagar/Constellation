namespace Constellation.Application.Attendance.Plans.UpdateAttendancePlanStatus;

using Abstractions.Messaging;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;

public sealed record UpdateAttendancePlanStatusCommand(
    AttendancePlanId PlanId,
    AttendancePlanStatus NewStatus)
    : ICommand;
