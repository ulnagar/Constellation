namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetChartDataForEnrolmentStatus;

using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using System.Collections.Generic;

public sealed record ChartResponse(
    EnrolmentPeriodId PeriodId,
    string PeriodName,
    Dictionary<string, int> ChartData);