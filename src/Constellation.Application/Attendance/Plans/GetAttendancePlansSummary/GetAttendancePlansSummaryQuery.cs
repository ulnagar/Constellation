namespace Constellation.Application.Attendance.Plans.GetAttendancePlansSummary;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAttendancePlansSummaryQuery()
    : IQuery<List<AttendancePlanSummaryResponse>>;
