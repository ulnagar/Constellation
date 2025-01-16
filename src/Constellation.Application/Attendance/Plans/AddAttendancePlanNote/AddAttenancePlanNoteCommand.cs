namespace Constellation.Application.Attendance.Plans.AddAttendancePlanNote;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record AddAttendancePlanNoteCommand(
    AttendancePlanId PlanId,
    string Message)
    : ICommand;