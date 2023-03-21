#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;

internal sealed class TutorialEnrolmentRepository : ITutorialEnrolmentRepository
{
    private readonly AppDbContext _dbContext;

    public TutorialEnrolmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TutorialEnrolment?> GetById(
        TutorialEnrolmentId id,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialEnrolment>()
            .FirstOrDefaultAsync(enrolment => enrolment.Id == id, cancellationToken);

    public async Task<List<TutorialEnrolment>> GetActiveForTutorial(
        GroupTutorialId tutorialId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialEnrolment>()
            .Where(enrolment => !enrolment.IsDeleted && enrolment.TutorialId == tutorialId)
            .ToListAsync(cancellationToken);

    public async Task<int?> GetCountForTutorial(
        GroupTutorialId tutorialId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialEnrolment>()
            .CountAsync(enrolment => enrolment.TutorialId == tutorialId && !enrolment.IsDeleted, cancellationToken);


    public void Insert(TutorialEnrolment enrolment) =>
        _dbContext.Set<TutorialEnrolment>().Add(enrolment);
}
