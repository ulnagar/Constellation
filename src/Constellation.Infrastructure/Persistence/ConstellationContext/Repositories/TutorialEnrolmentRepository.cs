#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.GroupTutorials;
using Microsoft.EntityFrameworkCore;

internal sealed class TutorialEnrolmentRepository : ITutorialEnrolmentRepository
{
    private readonly AppDbContext _dbContext;

    public TutorialEnrolmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TutorialEnrolment?> GetById(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialEnrolment>()
            .FirstOrDefaultAsync(tutorial => tutorial.Id == id, cancellationToken);

    public void Insert(TutorialEnrolment enrolment) =>
        _dbContext.Set<TutorialEnrolment>().Add(enrolment);
}
