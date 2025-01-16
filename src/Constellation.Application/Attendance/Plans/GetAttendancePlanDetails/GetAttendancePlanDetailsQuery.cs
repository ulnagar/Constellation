namespace Constellation.Application.Attendance.Plans.GetAttendancePlanDetails;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record GetAttendancePlanDetailsQuery(
    AttendancePlanId PlanId)
    : IQuery<AttendancePlanDetailsResponse>;
