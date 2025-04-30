namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlansSummaryForSchool;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlansSummary;
using GetAttendancePlansSummary;
using System.Collections.Generic;

public sealed record GetAttendancePlansSummaryForSchoolQuery(
    string SchoolCode)
    : IQuery<List<AttendancePlanSummaryResponse>>;