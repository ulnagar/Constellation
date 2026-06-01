namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Identifiers;
using Core.Models.StudentOnboarding.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class ApplicantRepository
: IApplicantRepository
{
    private readonly AppDbContext _context;

    public ApplicantRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> DoesApplicantIdExist(
        ApplicantId applicantId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Applicant>()
            .AnyAsync(entry => entry.Id == applicantId, cancellationToken);
}
