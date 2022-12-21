namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.GroupTutorials;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

internal sealed class TutorialTeacherRepository : ITutorialTeacherRepository
{
    private readonly AppDbContext _dbContext;

    public TutorialTeacherRepository(AppDbContext context)
    {
        _dbContext = context;
    }

    public async Task<List<TutorialTeacher>> GetActiveForTutorial(
        Guid tutorialId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialTeacher>()
            .Where(teacher => !teacher.IsDeleted && teacher.TutorialId == tutorialId)
            .ToListAsync(cancellationToken);

    public async Task<TutorialTeacher> GetById(Guid Id, CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialTeacher>()
            .FirstOrDefaultAsync(teacher => teacher.Id == Id, cancellationToken);

    public async Task<int?> GetCountForTutorial(
        Guid tutorialId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialTeacher>()
            .CountAsync(teacher => teacher.TutorialId == tutorialId && !teacher.IsDeleted, cancellationToken);

    public void Insert(TutorialTeacher teacher) =>
        _dbContext
            .Set<TutorialTeacher>()
            .Add(teacher);
}
