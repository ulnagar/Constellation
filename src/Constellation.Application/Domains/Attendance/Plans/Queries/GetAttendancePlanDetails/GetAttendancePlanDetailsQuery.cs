namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlanDetails;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record GetAttendancePlanDetailsQuery(
    AttendancePlanId PlanId)
    : IQuery<AttendancePlanDetailsResponse>;
