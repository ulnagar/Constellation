namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Abstractions.Clock;
using Microsoft.EntityFrameworkCore;

internal class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public AssignmentRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<CanvasAssignment?> GetByCanvasId(
        int CanvasAssignmentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .Where(assignment => assignment.CanvasId == CanvasAssignmentId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<CanvasAssignment>> GetByCourseId(
        CourseId courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .Where(assignment => assignment.CourseId == courseId)
            .ToListAsync(cancellationToken);

    public async Task<CanvasAssignment> GetById(
        AssignmentId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
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
            .Where(assignment => 
                (assignment.DueDate >= today || (!assignment.LockDate.HasValue || assignment.LockDate.Value > today)) &&
                (!assignment.UnlockDate.HasValue || assignment.UnlockDate.Value <= today))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CanvasAssignment>> GetAllCurrentAndFuture(
        CancellationToken cancellationToken = default)
    {
        DateTime today = DateTime.Today;

        return await _context
            .Set<CanvasAssignment>()
            .Where(assignment =>
                assignment.DueDate >= today || 
                (!assignment.LockDate.HasValue || assignment.LockDate.Value > today))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CanvasAssignment>> GetAllDueForUpload(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .Where(assignment => 
                assignment.DelayForwarding &&
                assignment.ForwardingDate == _dateTime.Today)
            .ToListAsync(cancellationToken);
}
