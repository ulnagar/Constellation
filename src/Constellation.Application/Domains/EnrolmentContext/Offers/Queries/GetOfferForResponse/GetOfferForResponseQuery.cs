namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetOfferForResponse;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record GetOfferForResponseQuery(
    OfferId OfferId)
    : IQuery<OfferResponse>;
