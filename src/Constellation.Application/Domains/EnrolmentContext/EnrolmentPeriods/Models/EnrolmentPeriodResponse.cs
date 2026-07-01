namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;

using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;

public sealed record EnrolmentPeriodResponse(
    EnrolmentPeriodId Id,
    string Label,
    DateTimeOffset OpenAt,
    DateTimeOffset ClosedAt,
    PeriodStatus Status);