namespace Constellation.Application.Domains.Attendance.Plans.Commands.CreateAttendancePlanVersion;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;


public sealed record CreateAttendancePlanVersionCommand(
    AttendancePlanId PlanId)
    : ICommand<AttendancePlanId>;