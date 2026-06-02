namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.StudentOnboarding.Enums;
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

    public async Task<Application?> GetApplicationById(
        ApplicationId applicationId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .FirstOrDefaultAsync(entry => entry.Id == applicationId,
                cancellationToken);

    public async Task<List<Application>> GetApplicationsByApplicantId(
        ApplicantId applicantId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(entry => entry.ApplicantId == applicantId)
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetApplicationsByParentId(
        ParentId parentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(entry => entry.Parents.Any(parent => parent.Id == parentId))
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetApplicationsByProgram(
        Program program,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(entry => entry.Program == program)
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetAllApplications(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetCurrentApplications(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .ToListAsync(cancellationToken);

    public async Task<bool> DoesApplicationIdExist(
        ApplicationId applicationId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .AnyAsync(entry => entry.Id == applicationId, cancellationToken);

    public void Insert(Application application) => _context.Set<Application>().Add(application);
}
