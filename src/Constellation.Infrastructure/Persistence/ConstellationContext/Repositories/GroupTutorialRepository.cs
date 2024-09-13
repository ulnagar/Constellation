#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Core.Abstractions.Clock;
using Microsoft.EntityFrameworkCore;

internal sealed class GroupTutorialRepository : IGroupTutorialRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IDateTimeProvider _dateTime;

    public GroupTutorialRepository(
        AppDbContext dbContext,
        IDateTimeProvider dateTime)
    {
        _dbContext = dbContext;
        _dateTime = dateTime;
    }

    public async Task<GroupTutorial?> GetById(
    GroupTutorialId id,
    CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Where(tutorial => tutorial.Id == id)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<GroupTutorial?> GetByName(
        string name,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .AsSplitQuery()
            .FirstOrDefaultAsync(tutorial => tutorial.Name == name, cancellationToken);

    public async Task<List<GroupTutorial>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<List<GroupTutorial>> GetActive(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Where(tutorial => 
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<List<GroupTutorial>> GetFuture(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Where(tutorial => tutorial.StartDate > _dateTime.Today)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<List<GroupTutorial>> GetInactive(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Where(tutorial => tutorial.EndDate < _dateTime.Today)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<List<GroupTutorial>> GetAllWhereAccessExpired(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Where(tutorial =>
                tutorial.Enrolments.Any(enrol =>
                    !enrol.IsDeleted &&
                    enrol.EffectiveTo < _dateTime.Today) ||
                tutorial.Teachers.Any(member =>
                    !member.IsDeleted &&
                    member.EffectiveTo < _dateTime.Today))
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<List<GroupTutorial>> GetAllActiveForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Where(tutorial =>
                tutorial.Enrolments.Any(enrol =>
                    !enrol.IsDeleted &&
                    enrol.StudentId == studentId) &&
                tutorial.EndDate >= _dateTime.Today &&
                !tutorial.IsDeleted)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public void Insert(GroupTutorial tutorial) =>
        _dbContext.Set<GroupTutorial>().Add(tutorial);
}
