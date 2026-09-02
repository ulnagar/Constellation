namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetChartDataForEnrolmentStatus;

using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.Offer.Enums;
using System.Collections.Generic;

public sealed record ChartResponse(
    EnrolmentPeriodId PeriodId,
    string PeriodName,
    Dictionary<OfferStatus, int> ChartData);