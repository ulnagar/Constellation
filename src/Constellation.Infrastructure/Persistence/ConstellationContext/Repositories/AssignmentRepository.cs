namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Abstractions.Clock;
using Core.Models.Assessments.Archive;
using Core.Models.Assessments.Archive.Identifiers;
using Core.Models.Assessments.Archive.Repositories;
using Core.Models.Subjects.Identifiers;
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

    public async Task<CanvasAssignment?> GetById(
        AssignmentId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .FirstOrDefaultAsync(assignment => assignment.Id == id, cancellationToken);

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

    public async Task<List<CanvasAssignment>> GetFromCurrentYear(
        CancellationToken cancellationToken = default)
    {
        DateTime Jan1 = _dateTime.FirstDayOfYear.ToDateTime(TimeOnly.MinValue);

        return await _context
            .Set<CanvasAssignment>()
            .Where(assignment =>
                assignment.DueDate >= Jan1 ||
                (!assignment.LockDate.HasValue || assignment.LockDate.Value > Jan1))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CanvasAssignment>> GetExpiredFromCurrentYear(
        CancellationToken cancellationToken = default)
    {
        DateTime jan1 = _dateTime.FirstDayOfYear.ToDateTime(TimeOnly.MinValue);

        return await _context
            .Set<CanvasAssignment>()
            .Where(assignment =>
                assignment.DueDate >= jan1 || 
                (assignment.LockDate.HasValue && assignment.LockDate.Value > jan1))
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

    public async Task<List<CanvasAssignment>> GetForCleanup(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CanvasAssignment>()
            .Where(assignment => assignment.DueDate <= _dateTime.Today.AddMonths(-18).ToDateTime(TimeOnly.MinValue))
            .ToListAsync(cancellationToken);
}
