namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class ConsentRepository : IConsentRepository
{
    private readonly AppDbContext _context;

    public ConsentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool?> IsMostRecentResponse(
        ConsentId consentId,
        CancellationToken cancellationToken = default)
    {
        Consent consent = await _context.Set<Consent>()
            .FirstOrDefaultAsync(entry => entry.Id == consentId, cancellationToken);

        if (consent is null)
            return null;

        return !await _context
            .Set<Consent>()
            .AnyAsync(entry =>
                entry.StudentId == consent.StudentId &&
                entry.ApplicationId == consent.ApplicationId &&
                entry.ProvidedAt > consent.ProvidedAt,
                cancellationToken);
    }

    public async Task<Application> GetApplicationById(
        ApplicationId applicationId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(application => application.Id == applicationId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Application>> GetAllActiveApplications(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<Application>()
            .Where(application => !application.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetAllApplications(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetApplicationsWithoutRequiredConsent(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(application =>
                !application.IsDeleted &&
                !application.ConsentRequired)
            .ToListAsync(cancellationToken);
    public async Task<List<Application>> GetApplicationWithConsentForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(application => application.Consents.Any(consent =>
                consent.StudentId == studentId))
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetApplicationsByTransactionId(
        ConsentTransactionId transactionId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(application => application.Consents.Any(consent =>
                consent.TransactionId == transactionId))
            .ToListAsync(cancellationToken);

    public async Task<List<ConsentRequirement>> GetRequirementsForApplication(
        ApplicationId applicationId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ConsentRequirement>()
            .Where(requirement => 
                requirement.ApplicationId == applicationId && 
                !requirement.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<ConsentRequirement> GetRequirementById(
        ConsentRequirementId requirementId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ConsentRequirement>()
            .Where(requirement => requirement.Id == requirementId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<ConsentRequirement>> GetAllRequirements(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ConsentRequirement>()
            .ToListAsync(cancellationToken);

    public async Task<List<CourseConsentRequirement>> GetRequirementsForCourse(
        CourseId courseId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ConsentRequirement>()
            .OfType<CourseConsentRequirement>()
            .Where(requirement => 
                requirement.CourseId == courseId && 
                !requirement.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<GradeConsentRequirement>> GetRequirementsForGrade(
        Grade grade, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ConsentRequirement>()
            .OfType<GradeConsentRequirement>()
            .Where(requirement => 
                requirement.Grade == grade &&
                !requirement.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<StudentConsentRequirement>> GetRequirementsForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ConsentRequirement>()
            .OfType<StudentConsentRequirement>()
            .Where(requirement => 
                requirement.StudentId == studentId &&
                !requirement.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<Transaction> GetTransactionById(
        ConsentTransactionId transactionId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Transaction>()
            .FirstOrDefaultAsync(
                transaction => transaction.Id == transactionId,
                cancellationToken);

    public void Insert(Application application) => _context.Set<Application>().Add(application);
    
    public void Insert(ConsentRequirement requirement) => _context.Set<ConsentRequirement>().Add(requirement);

    public void Insert(Transaction transaction) => _context.Set<Transaction>().Add(transaction);
}
