namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetRecentlyCompletedPlans;

using Core.Models.Attendance.Identifiers;
using Core.ValueObjects;

public sealed record CompletedPlansResponse(
    AttendancePlanId PlanId,
    Name Student,
    string DisplayName);