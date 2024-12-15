namespace Constellation.Application.Attendance.Plans.GetAttendancePlanForSubmit;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record GetAttendancePlanForSubmitQuery(
    AttendancePlanId PlanId)
: IQuery<AttendancePlanEntry>;