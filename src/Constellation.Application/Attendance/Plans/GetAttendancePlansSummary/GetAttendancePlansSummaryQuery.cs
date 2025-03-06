namespace Constellation.Application.Attendance.Plans.GetAttendancePlansSummary;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAttendancePlansSummaryQuery(
    AttendancePlanStatusFilter Filter = AttendancePlanStatusFilter.All)
    : IQuery<List<AttendancePlanSummaryResponse>>;

public enum AttendancePlanStatusFilter
{
    All,
    InProgress,
    Current,
    Expired
}