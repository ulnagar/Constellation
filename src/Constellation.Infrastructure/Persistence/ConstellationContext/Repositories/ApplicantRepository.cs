namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.StudentOnboarding.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Identifiers;
using Core.Models.StudentOnboarding.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

internal sealed class OnboardingRepository
    : IOnboardingRepository
{
    private readonly AppDbContext _context;

    public OnboardingRepository(
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

    public async Task<Applicant?> GetApplicantById(
        ApplicantId applicantId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Applicant>()
            .FirstOrDefaultAsync(entry => entry.Id == applicantId, cancellationToken);

    public async Task<Applicant?> GetApplicantByStudentReferenceNumber(
        StudentReferenceNumber srn,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Applicant>()
            .FirstOrDefaultAsync(entry => entry.StudentReferenceNumber == srn, cancellationToken);

    public async Task<Applicant?> GetApplicantByEmailAddress(
        EmailAddress emailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Applicant>()
            .FirstOrDefaultAsync(entry => entry.EmailAddress == emailAddress, cancellationToken);

    public void Insert(Application application) => _context.Set<Application>().Add(application);
    public void Insert(Applicant applicant) => _context.Set<Applicant>().Add(applicant);
}
