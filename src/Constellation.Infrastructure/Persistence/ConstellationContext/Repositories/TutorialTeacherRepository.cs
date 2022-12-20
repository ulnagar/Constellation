namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.GroupTutorials;
using Microsoft.EntityFrameworkCore;

internal sealed class TutorialTeacherRepository : ITutorialTeacherRepository
{
    private readonly AppDbContext _dbContext;

    public TutorialTeacherRepository(AppDbContext context)
    {
        _dbContext = context;
    }

    public async Task<TutorialTeacher> GetById(Guid Id, CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<TutorialTeacher>()
            .FirstOrDefaultAsync(teacher => teacher.Id == Id, cancellationToken);

    public void Insert(TutorialTeacher teacher) =>
        _dbContext
            .Set<TutorialTeacher>()
            .Add(teacher);
}
