namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.CountOffersInReviewingResponseStatus;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;

internal sealed class CountOffersInReviewingResponseStatusQueryHandler
: IQueryHandler<CountOffersInReviewingResponseStatusQuery, int>
{
    private readonly IEnrolmentOfferRepository _offerRepository;

    public CountOffersInReviewingResponseStatusQueryHandler(
        IEnrolmentOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<int>> Handle(CountOffersInReviewingResponseStatusQuery request, CancellationToken cancellationToken)
    {
        return await _offerRepository.CountReviewingResponseOffers(cancellationToken);
    }
}
