namespace Constellation.Application.Domains.Attendance.Plans.Commands.SubmitAttendancePlan;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record SubmitAttendancePlanCommand(
    AttendancePlanId PlanId)
    : ICommand;
