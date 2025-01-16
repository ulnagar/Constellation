namespace Constellation.Application.Attendance.Plans.GetAttendancePlansSummary;

using Core.Enums;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.ValueObjects;

public sealed record AttendancePlanSummaryResponse(
    AttendancePlanId PlanId,
    Name Student,
    Grade Grade,
    string School,
    AttendancePlanStatus Status,
    double OverallPercentage);