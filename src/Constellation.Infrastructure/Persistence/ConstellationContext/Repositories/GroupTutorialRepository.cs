#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;

internal sealed class GroupTutorialRepository : IGroupTutorialRepository
{
    private readonly AppDbContext _dbContext;

    public GroupTutorialRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GroupTutorial?> GetById(
    GroupTutorialId id,
    CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Include(tutorial => tutorial.Enrolments)
            .Include(tutorial => tutorial.Teachers)
            .Include(tutorial => tutorial.Rolls)
            .ThenInclude(roll => roll.Students)
            .Where(tutorial => tutorial.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<GroupTutorial?> GetByName(
        string name,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Include(tutorial => tutorial.Enrolments)
            .Include(tutorial => tutorial.Teachers)
            .Include(tutorial => tutorial.Rolls)
            .ThenInclude(roll => roll.Students)
            .FirstOrDefaultAsync(tutorial => tutorial.Name == name, cancellationToken);

    public async Task<List<GroupTutorial>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Include(tutorial => tutorial.Enrolments)
            .Include(tutorial => tutorial.Teachers)
            .Include(tutorial => tutorial.Rolls)
            .ThenInclude(roll => roll.Students)
            .ToListAsync(cancellationToken);

    public async Task<List<GroupTutorial>> GetAllWhereAccessExpired(
        CancellationToken cancellationToken = default)
    {
        var dateOnlyToday = DateOnly.FromDateTime(DateTime.Today);

        return await _dbContext
            .Set<GroupTutorial>()
            .Include(tutorial => tutorial.Enrolments)
            .Include(tutorial => tutorial.Teachers)
            .Include(tutorial => tutorial.Rolls)
            .ThenInclude(roll => roll.Students)
            .Where(tutorial =>
                tutorial.Enrolments.Any(enrol =>
                    !enrol.IsDeleted &&
                    enrol.EffectiveTo < dateOnlyToday) ||
                tutorial.Teachers.Any(member =>
                    !member.IsDeleted &&
                    member.EffectiveTo < dateOnlyToday))
            .ToListAsync(cancellationToken);
    }

    public void Insert(GroupTutorial tutorial) =>
        _dbContext.Set<GroupTutorial>().Add(tutorial);
}
