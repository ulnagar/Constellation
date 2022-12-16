#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.GroupTutorials;
using Microsoft.EntityFrameworkCore;

internal sealed class GroupTutorialRepository : IGroupTutorialRepository
{
    private readonly AppDbContext _dbContext;

    public GroupTutorialRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GroupTutorial?> GetWholeAggregate(
        Guid id, 
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .AsSplitQuery()
            .Include(tutorial => tutorial.Enrolments)
            .Include(tutorial => tutorial.Teachers)
            .Include(tutorial => tutorial.Rolls)
            .ThenInclude(roll => roll.Students)
            .Where(tutorial => tutorial.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(GroupTutorial tutorial) =>
        _dbContext.Set<GroupTutorial>().Add(tutorial);
}
