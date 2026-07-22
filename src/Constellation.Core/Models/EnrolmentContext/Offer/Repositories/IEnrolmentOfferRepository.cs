namespace Constellation.Core.Models.EnrolmentContext.Offer.Repositories;

using EnrolmentPeriod.Identifiers;
using Identifiers;

public interface IEnrolmentOfferRepository
{
    Task<List<Offer>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Offer>> GetForPeriod(EnrolmentPeriodId periodId, CancellationToken cancellationToken = default);
    Task<Offer?> GetById(OfferId offerId, CancellationToken cancellationToken = default);

    void Insert(Offer offer);
}
