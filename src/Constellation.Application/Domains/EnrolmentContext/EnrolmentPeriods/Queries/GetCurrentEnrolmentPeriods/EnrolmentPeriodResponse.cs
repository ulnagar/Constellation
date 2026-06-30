namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;

using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using System;

public sealed record EnrolmentPeriodResponse(
    EnrolmentPeriodId Id,
    string Label,
    DateTimeOffset OpenAt,
    DateTimeOffset ClosedAt,
    PeriodStatus Status);