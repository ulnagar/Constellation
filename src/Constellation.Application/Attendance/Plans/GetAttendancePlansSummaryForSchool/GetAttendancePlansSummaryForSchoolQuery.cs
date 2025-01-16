namespace Constellation.Application.Attendance.Plans.GetAttendancePlansSummaryForSchool;

using Abstractions.Messaging;
using GetAttendancePlansSummary;
using System.Collections.Generic;

public sealed record GetAttendancePlansSummaryForSchoolQuery(
    string SchoolCode)
    : IQuery<List<AttendancePlanSummaryResponse>>;