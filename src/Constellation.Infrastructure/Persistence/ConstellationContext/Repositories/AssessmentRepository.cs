namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

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
}