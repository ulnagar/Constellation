namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetOfferSummaryForPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetOfferSummaryForPeriodQuery(
    EnrolmentPeriodId PeriodId)
    : IQuery<List<EnrolmentOfferSummaryResponse>>;