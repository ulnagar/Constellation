namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetOffersForPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetOffersForPeriodQuery(
    EnrolmentPeriodId PeriodId)
    : IQuery<List<EnrolmentOfferResponse>>;