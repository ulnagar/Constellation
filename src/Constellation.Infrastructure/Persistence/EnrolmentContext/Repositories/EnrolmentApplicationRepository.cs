namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Microsoft.EntityFrameworkCore;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

internal sealed class EnrolmentApplicationRepository
: IEnrolmentApplicationRepository
{
    private readonly EnrolmentDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public EnrolmentApplicationRepository(
        EnrolmentDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Application?> GetApplicationById(
        ApplicationId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .FirstOrDefaultAsync(
                application => application.Id == id,
                cancellationToken);

    public async Task<List<Application>> GetApplicationsByPeriod(
        EnrolmentPeriodId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(entry => entry.PeriodId == id)
            .ToListAsync(cancellationToken);

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

    public void Insert(Application application) => _context.Set<Application>().Add(application);
    public void Insert(EnrolmentPeriod period) => _context.Set<EnrolmentPeriod>().Add(period);
}
