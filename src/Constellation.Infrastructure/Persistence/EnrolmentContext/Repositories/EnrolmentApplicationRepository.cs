namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
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

    public async Task<List<EnrolmentPeriod>> GetCurrentEnrolmentPeriods(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentPeriod>()
            .Where(period =>
                period.Status == PeriodStatus.Open
                && period.OpenAt < _dateTime.Now
                && period.ClosedAt > _dateTime.Now)
            .ToListAsync(cancellationToken);

    public void Insert(Application application) => _context.Set<Application>().Add(application);
}
