namespace Constellation.Application.Attendance.Plans.GetRecentlyCompletedPlans;

using Core.Models.Attendance.Identifiers;
using Core.ValueObjects;

public sealed record CompletedPlansResponse(
    AttendancePlanId PlanId,
    Name Student);