namespace Constellation.Application.Offerings.GetAllOfferingSummaries;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllOfferingSummariesQuery()
    : IQuery<List<OfferingSummaryResponse>>;
