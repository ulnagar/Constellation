namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

internal sealed class EnrolmentApplicationRepository
: IEnrolmentApplicationRepository
{
    private readonly EnrolmentDbContext _context;

    public EnrolmentApplicationRepository(
        EnrolmentDbContext context)
    {
        _context = context;
    }

    public async Task<Application?> GetApplicationById(
        ApplicationId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .FirstOrDefaultAsync(
                application => application.Id == id,
                cancellationToken);

    public void Insert(Application application) => _context.Set<Application>().Add(application);
}
