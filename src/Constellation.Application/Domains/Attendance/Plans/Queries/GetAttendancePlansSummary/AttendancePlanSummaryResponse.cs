namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlansSummary;

using Core.Enums;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.ValueObjects;
using System;

public sealed record AttendancePlanSummaryResponse(
    AttendancePlanId PlanId,
    Name Student,
    Grade Grade,
    string School,
    DateTime CreatedAt,
    AttendancePlanStatus Status,
    double OverallPercentage);