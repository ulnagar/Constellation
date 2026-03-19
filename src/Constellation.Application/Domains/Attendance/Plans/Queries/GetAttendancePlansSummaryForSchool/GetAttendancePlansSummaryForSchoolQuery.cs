namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlansSummaryForSchool;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlansSummary;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetAttendancePlansSummaryForSchoolQuery(
    SchoolCode SchoolCode)
    : IQuery<List<AttendancePlanSummaryResponse>>;