namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;

using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;

public sealed record EnrolmentPeriodResponse(
    EnrolmentPeriodId Id,
    string Label,
    string Year,
    DateTimeOffset OpenAt,
    DateTimeOffset ClosedAt,
    PeriodStatus Status,
    Program Program,
    IReadOnlyList<EnrolmentCourse> AvailableCourses,
    bool IsSuspended,
    string? SuspendedReason);