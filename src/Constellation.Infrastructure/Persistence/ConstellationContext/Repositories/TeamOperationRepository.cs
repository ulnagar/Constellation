namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Operations;
using Core.Models.Operations.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class TeamOperationRepository : ITeamOperationRepository
{
    private readonly ConstellationDbContext _context;

    public TeamOperationRepository(
        ConstellationDbContext context)
    {
        _context = context;
    }

    public async Task<TeamOperation> GetById(
        int id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TeamOperation>()
            .FirstOrDefaultAsync(operation => operation.Id == id,
                cancellationToken);

    public async Task<List<TeamOperation>> GetDue(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TeamOperation>()
            .Where(operation =>
                !operation.IsCompleted &&
                !operation.IsDeleted &&
                operation.ScheduledFor.Date == DateTime.Today)
            .ToListAsync(cancellationToken);

    public async Task<List<TeamOperation>> GetOverdue(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TeamOperation>()
            .Where(operation =>
                !operation.IsCompleted &&
                !operation.IsDeleted &&
                operation.ScheduledFor.Date < DateTime.Today)
            .ToListAsync(cancellationToken);

    public void Insert(TeamOperation operation) =>
        _context.Set<TeamOperation>().Add(operation);
}