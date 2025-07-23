namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Tutorials.ValueObjects;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class TutorialRepository : ITutorialRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public TutorialRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Tutorial> GetById(
        TutorialId tutorialId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .FirstOrDefaultAsync(tutorial => tutorial.Id == tutorialId,
                cancellationToken);

    public async Task<List<Tutorial>> GetAll(
        CancellationToken cancellationToken = default)
    {
        DateOnly startOfYear = _dateTime.FirstDayOfYear;

        return await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate >= startOfYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tutorial>> GetAllActive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today &&
                tutorial.Sessions.Any(session => !session.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Tutorial>> GetInactive(
        CancellationToken cancellationToken = default)
    {
        return await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.IsDeleted &&
                (tutorial.StartDate >= _dateTime.FirstDayOfYear ||
                 tutorial.Sessions.Count == 0 ||
                 tutorial.Sessions.All(session => session.IsDeleted)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tutorial>> GetActiveForTeacher(
        StaffId staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today &&
                tutorial.Sessions.Any(session =>
                    !session.IsDeleted &&
                    session.StaffId == staffId))
            .ToListAsync(cancellationToken);

    public async Task<bool> DoesTutorialAlreadyExist(
        TutorialName name,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .AnyAsync(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.Name == name &&
                ((tutorial.StartDate <= endDate && tutorial.StartDate >= startDate) || // DB Tutorial starts during new tutorial
                (tutorial.EndDate <= endDate && tutorial.EndDate >= startDate) || //DB Tutorial ends during new tutorial
                (tutorial.StartDate <= startDate && tutorial.EndDate >= endDate)), // DB Tutorial starts before and ends after new tutorial
                cancellationToken);

    public void Insert(Tutorial tutorial) =>
        _context.Set<Tutorial>().Add(tutorial);
}
