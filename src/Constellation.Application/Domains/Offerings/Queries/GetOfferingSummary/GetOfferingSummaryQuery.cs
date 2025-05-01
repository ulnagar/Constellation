namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Offerings.Models;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record GetOfferingSummaryQuery(
    OfferingId OfferingId)
    : IQuery<OfferingSummaryResponse>;