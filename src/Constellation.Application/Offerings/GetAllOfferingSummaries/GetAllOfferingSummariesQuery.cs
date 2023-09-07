namespace Constellation.Application.Offerings.GetAllOfferingSummaries;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Offerings.Models;
using System.Collections.Generic;

public sealed record GetAllOfferingSummariesQuery()
    : IQuery<List<OfferingSummaryResponse>>;
