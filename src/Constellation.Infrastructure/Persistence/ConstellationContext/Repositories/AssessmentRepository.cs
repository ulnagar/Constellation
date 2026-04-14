namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Students.Identifiers;
using Core.Models.Assessments;
using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class AssessmentRepository : IAssessmentRepository
{
    private readonly AppDbContext _context;

    public AssessmentRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<Assessment?> GetById(
        AssessmentId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Assessment>()
            .FirstOrDefaultAsync(assessment => assessment.Id == id, cancellationToken);

    public async Task<List<Assessment>> GetCurrentAssessments(
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context
            .Set<Assessment>()
            .Where(a => 
                a.AvailableFrom <= now 
                && a.AvailableTo >= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Assessment>> GetForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Assessment>()
            .Where(a => a.Students.Any(s => s.StudentId == studentId))
            .ToListAsync(cancellationToken);

    public async Task<List<Assessment>> GetCurrentForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return await _context
            .Set<Assessment>()
            .Where(a =>
                a.AvailableFrom <= now
                && a.AvailableTo >= now
                && a.Students.Any(s => s.StudentId == studentId))
            .ToListAsync(cancellationToken);
    }

    public void Insert(Assessment assessment) => _context.Set<Assessment>().Add(assessment);
}