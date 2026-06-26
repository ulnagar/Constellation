namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Application.Domains.EnrolmentContext.Interfaces;

internal sealed class EnrolmentUnitOfWork : IEnrolmentUnitOfWork
{
    private readonly EnrolmentDbContext _context;

    public EnrolmentUnitOfWork(
        EnrolmentDbContext context)
    {
        _context = context;
    }

    public async Task CompleteAsync(CancellationToken token = default) => await _context.SaveChangesAsync(token);
}
