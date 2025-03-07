namespace Constellation.Application.Attendance.Plans.RejectAttendancePlan;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance.Identifiers;

public sealed record RejectAttendancePlanCommand(
    AttendancePlanId PlanId,
    string Comment,
    bool SendEmail)
    : ICommand;