#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;

internal sealed class TeamRepository : ITeamRepository
{
    private readonly AppDbContext _dbContext;

    public TeamRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Team>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Team>()
            .ToListAsync(cancellationToken);

    public async Task<Team?> GetById(
        Guid teamId, 
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Team>()
            .FirstOrDefaultAsync(team => team.Id == teamId, cancellationToken);

    public async Task<Team?> GetByName(
        string Name, 
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Team>()
            .FirstOrDefaultAsync(team => team.Name.Contains(Name), cancellationToken);

    public async Task<Guid?> GetIdByOffering(
        string offeringName, 
        string offeringYear, 
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Team>()
            .Where(team => team.Name.Contains(offeringName) && team.Name.Contains(offeringYear))
            .Select(team => team.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<string?> GetLinkById(
        Guid teamId, 
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Team>()
            .Where(team => team.Id == teamId)
            .Select(team => team.Link)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<string?> GetLinkByOffering(
        string offeringName, 
        string offeringYear, 
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Team>()
            .Where(team => team.Name.Contains(offeringName) && team.Name.Contains(offeringYear))
            .Select(team => team.Link)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(Team team) =>
        _dbContext.Set<Team>().Add(team);

    public void Remove(Team team) =>
        _dbContext.Set<Team>().Remove(team);
}
