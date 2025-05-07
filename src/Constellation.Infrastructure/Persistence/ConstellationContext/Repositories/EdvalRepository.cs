namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Domains.Edval.Repositories;
using Core.Models.Edval;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public sealed class EdvalRepository : IEdvalRepository
{
    private readonly AppDbContext _context;

    public EdvalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task ClearClasses(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EdvalClass>()
            .ExecuteDeleteAsync(cancellationToken);
    
    public async Task ClearClassMemberships(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EdvalClassMembership>()
            .ExecuteDeleteAsync(cancellationToken);
    

    public async Task ClearStudents(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EdvalStudent>()
            .ExecuteDeleteAsync(cancellationToken);

    public async Task ClearTeachers(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EdvalTeacher>()
            .ExecuteDeleteAsync(cancellationToken);
    
    public async Task ClearTimetables(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EdvalTimetable>()
            .ExecuteDeleteAsync(cancellationToken);

    public void Insert(EdvalClass entity) => _context.Add(entity);

    public void Insert(EdvalClassMembership entity) => _context.Add(entity);

    public void Insert(EdvalStudent entity) => _context.Add(entity);

    public void Insert(EdvalTeacher entity) => _context.Add(entity);

    public void Insert(EdvalTimetable entity) => _context.Add(entity);
}
