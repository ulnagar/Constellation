namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;
using Core.Models.EnrolmentContext.Application.Identifiers;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class EnrolmentOfferRepository
    : IEnrolmentOfferRepository
{
    private readonly EnrolmentDbContext _context;

    public EnrolmentOfferRepository(
        EnrolmentDbContext context)
    {
        _context = context;
    }

    public async Task<List<Offer>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context.Set<Offer>()
            .ToListAsync(cancellationToken);

    public async Task<List<Offer>> GetForPeriod(
        EnrolmentPeriodId periodId,
        CancellationToken cancellationToken = default) =>
        await _context.Set<Offer>()
            .Where(offer => offer.PeriodId == periodId)
            .ToListAsync(cancellationToken);

    public async Task<Offer?> GetById(
        OfferId offerId,
        CancellationToken cancellationToken = default) =>
        await _context.Set<Offer>()
            .FirstOrDefaultAsync(
                offer => offer.Id == offerId,
                cancellationToken);

    public async Task<Offer?> GetByApplicationId(
        ApplicationId applicationId,
        CancellationToken cancellationToken = default) =>
        await _context.Set<Offer>()
            .FirstOrDefaultAsync(
                offer => offer.ApplicationId == applicationId,
                cancellationToken);

    public async Task<List<Offer>> GetListFromIds(
        List<OfferId> offerIds,
        CancellationToken cancellationToken = default) =>
        await _context.Set<Offer>()
            .Where(offer => offerIds.Contains(offer.Id))
            .ToListAsync(cancellationToken);

    public async Task<int> CountPendingAcceptanceOffers(
        CancellationToken cancellationToken = default) =>
        await _context.Set<Offer>()
            .CountAsync(entry => entry.Status == OfferStatus.PendingAcceptance, cancellationToken);

    public async Task<int> CountReviewingResponseOffers(
        CancellationToken cancellationToken = default) =>
        await _context.Set<Offer>()
            .CountAsync(entry => entry.Status == OfferStatus.ReviewingResponse, cancellationToken);

    public void Insert(Offer offer) => _context.Set<Offer>().Add(offer);
}