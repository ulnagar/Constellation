namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.GroupTutorials;

internal sealed class TutorialRollRepository : ITutorialRollRepository
{
    private readonly AppDbContext _dbContext;

    public TutorialRollRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Insert(TutorialRoll roll) =>
        _dbContext.Set<TutorialRoll>().Add(roll);
}
