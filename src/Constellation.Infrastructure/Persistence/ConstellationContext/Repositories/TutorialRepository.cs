namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

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

    public void Insert(Tutorial tutorial) =>
        _context.Set<Tutorial>().Add(tutorial);
}
