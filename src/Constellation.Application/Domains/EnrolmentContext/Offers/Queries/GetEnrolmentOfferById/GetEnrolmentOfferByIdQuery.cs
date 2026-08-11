namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetEnrolmentOfferById;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record GetEnrolmentOfferByIdQuery(
    OfferId OfferId)
    : IQuery<EnrolmentOfferDetailsResponse>;