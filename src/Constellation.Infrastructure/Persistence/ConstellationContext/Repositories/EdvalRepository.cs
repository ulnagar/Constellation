namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Domains.Edval.Repositories;
using Constellation.Core.Primitives;
using Core.Models.Edval;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public sealed class EdvalRepository : IEdvalRepository
{
    private readonly AppDbContext _context;

    public EdvalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountDifferences(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Difference>()
            .CountAsync(difference => difference.Ignored == false, cancellationToken);

    public async Task<int> CountIgnoredDifferences(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Difference>()
            .CountAsync(difference => difference.Ignored == true, cancellationToken);

    public async Task<Difference?> GetDifference(
        DifferenceId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Difference>()
            .FirstOrDefaultAsync(difference => difference.Id == id, cancellationToken);

    public async Task<List<Difference>> GetDifferences(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Difference>()
            .ToListAsync(cancellationToken);

    public async Task<List<EdvalClass>> GetClasses(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EdvalClass>()
            .ToListAsync(cancellationToken);

    public async Task<List<EdvalClassMembership>> GetClassMemberships(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EdvalClassMembership>()
            .ToListAsync(cancellationToken);

    public async Task<List<EdvalStudent>> GetStudents(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<EdvalStudent>()
            .ToListAsync(cancellationToken);

    public async Task<List<EdvalTeacher>> GetTeachers(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<EdvalTeacher>()
            .ToListAsync(cancellationToken);

    public async Task<List<EdvalTimetable>> GetTimetables(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<EdvalTimetable>()
            .ToListAsync(cancellationToken);

    public async Task<List<EdvalIgnore>> GetIgnoreRecords(
        EdvalDifferenceType? type,
        CancellationToken cancellationToken = default)
    {
        IQueryable<EdvalIgnore> records = _context
            .Set<EdvalIgnore>();

        if (type is not null)
        {
            records = records
                .Where(record => record.Type == type);
        }

        return await records.ToListAsync(cancellationToken);
    }

    public async Task ClearClasses(
        CancellationToken cancellationToken = default)
    {
        await _context
            .Set<EdvalClass>()
            .ExecuteDeleteAsync(cancellationToken);

        await ClearDifferences(EdvalDifferenceType.EdvalClass, cancellationToken);
    }


    public async Task ClearClassMemberships(
        CancellationToken cancellationToken = default)
    {
        await _context
            .Set<EdvalClassMembership>()
            .ExecuteDeleteAsync(cancellationToken);

        await ClearDifferences(EdvalDifferenceType.EdvalClassMembership, cancellationToken);
    }
        
    public async Task ClearStudents(
        CancellationToken cancellationToken = default)
    {
        await _context
            .Set<EdvalStudent>()
            .ExecuteDeleteAsync(cancellationToken);

        await ClearDifferences(EdvalDifferenceType.EdvalStudent, cancellationToken);
    }

    public async Task ClearTeachers(
        CancellationToken cancellationToken = default)
    {
        await _context
            .Set<EdvalTeacher>()
            .ExecuteDeleteAsync(cancellationToken);

        await ClearDifferences(EdvalDifferenceType.EdvalTeacher, cancellationToken);
    }
    
    public async Task ClearTimetables(
        CancellationToken cancellationToken = default)
    {
        await _context
            .Set<EdvalTimetable>()
            .ExecuteDeleteAsync(cancellationToken);

        await ClearDifferences(EdvalDifferenceType.EdvalTimetable, cancellationToken);
    }

    public async Task ClearDifferences(EdvalDifferenceType type, CancellationToken cancellationToken = default) =>
        await _context
            .Set<Difference>()
            .Where(difference => difference.Type == type)
            .ExecuteDeleteAsync(cancellationToken);

    public void Insert(EdvalClass entity) => _context.Add(entity);
    public void Insert(EdvalClassMembership entity) => _context.Add(entity);
    public void Insert(EdvalStudent entity) => _context.Add(entity);
    public void Insert(EdvalTeacher entity) => _context.Add(entity);
    public void Insert(EdvalTimetable entity) => _context.Add(entity);
    public void Insert(Difference entity) => _context.Add(entity);
    public void Insert(EdvalIgnore entity) => _context.Add(entity);

    public void Remove(EdvalIgnore entity) => _context.Remove(entity);

    public void AddIntegrationEvent(IIntegrationEvent integrationEvent) =>
        _context.AddIntegrationEvent(integrationEvent);
}
