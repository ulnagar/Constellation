﻿#nullable enable
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

    public async Task<List<GroupTutorial>> GetAllWithTeachersAndStudents(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .AsSplitQuery()
            .Include(tutorial => tutorial.Enrolments)
            .Include(tutorial => tutorial.Teachers)
            .ToListAsync(cancellationToken);

    public async Task<List<GroupTutorial>> GetAllWithTeachersAndStudentsWhereAccessExpired(
        CancellationToken cancellationToken = default)
    {
        var dateOnlyToday = DateOnly.FromDateTime(DateTime.Today);

        return await _dbContext
            .Set<GroupTutorial>()
            .AsSplitQuery()
            .Include(tutorial => tutorial.Enrolments)
            .Include(tutorial => tutorial.Teachers)
            .Where(tutorial =>
                tutorial.Enrolments.Any(enrol =>
                    !enrol.IsDeleted &&
                    enrol.EffectiveTo < dateOnlyToday) ||
                tutorial.Teachers.Any(member =>
                    !member.IsDeleted &&
                    member.EffectiveTo < dateOnlyToday))
            .ToListAsync(cancellationToken);
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

    public async Task<GroupTutorial?> GetById(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .FirstOrDefaultAsync(tutorial => tutorial.Id == id, cancellationToken);

    public async Task<GroupTutorial?> GetWithTeachersById(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Include(tutorial => tutorial.Teachers)
            .FirstOrDefaultAsync(tutorial => tutorial.Id == id, cancellationToken);

    public async Task<GroupTutorial?> GetWithStudentsById(
    Guid id,
    CancellationToken cancellationToken = default) =>
    await _dbContext
        .Set<GroupTutorial>()
        .Include(tutorial => tutorial.Enrolments)
        .FirstOrDefaultAsync(tutorial => tutorial.Id == id, cancellationToken);

    public async Task<GroupTutorial?> GetWithRollsById(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<GroupTutorial>()
            .Include(tutorial => tutorial.Rolls)
            .ThenInclude(roll => roll.Students)
            .FirstOrDefaultAsync(tutorial => tutorial.Id == id, cancellationToken);

    public void Insert(GroupTutorial tutorial) =>
        _dbContext.Set<GroupTutorial>().Add(tutorial);
}