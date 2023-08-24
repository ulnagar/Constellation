namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;

internal class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;

    public AssignmentRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CanvasAssignment>> GetByCourseId(
        int courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .Include(assignment => assignment.Submissions)
            .Where(assignment => assignment.CourseId == courseId)
            .ToListAsync(cancellationToken);

    public async Task<CanvasAssignment> GetById(
        AssignmentId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .Include(assignment => assignment.Submissions)
            .FirstOrDefaultAsync(assignment => assignment.Id == id, cancellationToken);

    public void Insert(CanvasAssignment entity) =>
        _context.Set<CanvasAssignment>().Add(entity);

    public async Task<bool> IsValidAssignmentId(
        AssignmentId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .AnyAsync(assignment => assignment.Id == id, cancellationToken);

    public async Task<List<CanvasAssignment>> GetAllCurrent(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;

        return await _context
            .Set<CanvasAssignment>()
            .Include(assignment => assignment.Submissions)
            .Where(assignment => 
                (assignment.DueDate >= today || (!assignment.LockDate.HasValue || assignment.LockDate.Value > today)) &&
                (!assignment.UnlockDate.HasValue || assignment.UnlockDate.Value <= today))
            .ToListAsync(cancellationToken);
    }
        
}
