namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class EnrolmentPeriodRepository
    : IEnrolmentPeriodRepository
{
    private readonly EnrolmentDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public EnrolmentPeriodRepository(
        EnrolmentDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<EnrolmentPeriod>> GetAllEnrolmentPeriods(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentPeriod>()
            .ToListAsync(cancellationToken);

    public async Task<List<EnrolmentPeriod>> GetCurrentEnrolmentPeriods(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentPeriod>()
            .Where(period =>
                _dateTime.Now >= period.OpenAt
                && _dateTime.Now < period.ClosedAt)
            .ToListAsync(cancellationToken);

    public async Task<EnrolmentPeriod?> GetEnrolmentPeriodById(
        EnrolmentPeriodId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentPeriod>()
            .FirstOrDefaultAsync(period => period.Id == id, cancellationToken);

    public void Insert(EnrolmentPeriod period) => _context.Set<EnrolmentPeriod>().Add(period);
}