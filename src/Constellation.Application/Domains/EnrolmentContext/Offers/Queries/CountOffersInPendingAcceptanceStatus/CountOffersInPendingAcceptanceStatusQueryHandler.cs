namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.CountOffersInPendingAcceptanceStatus;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;

internal sealed class CountOffersInPendingAcceptanceStatusQueryHandler
    : IQueryHandler<CountOffersInPendingAcceptanceStatusQuery, int>
{
    private readonly IEnrolmentOfferRepository _offerRepository;

    public CountOffersInPendingAcceptanceStatusQueryHandler(
        IEnrolmentOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<int>> Handle(CountOffersInPendingAcceptanceStatusQuery request, CancellationToken cancellationToken)
    {
        return await _offerRepository.CountPendingAcceptanceOffers(cancellationToken);
    }
}
