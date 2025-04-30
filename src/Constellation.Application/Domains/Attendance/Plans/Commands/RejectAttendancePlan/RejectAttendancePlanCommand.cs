namespace Constellation.Application.Domains.Attendance.Plans.Commands.RejectAttendancePlan;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance.Identifiers;

public sealed record RejectAttendancePlanCommand(
    AttendancePlanId PlanId,
    string Comment,
    bool SendEmail)
    : ICommand;