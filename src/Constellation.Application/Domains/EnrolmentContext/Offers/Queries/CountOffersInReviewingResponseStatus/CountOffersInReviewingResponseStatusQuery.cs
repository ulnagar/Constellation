namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.CountOffersInReviewingResponseStatus;

using Abstractions.Messaging;

public sealed record CountOffersInReviewingResponseStatusQuery()
    : IQuery<int>;
