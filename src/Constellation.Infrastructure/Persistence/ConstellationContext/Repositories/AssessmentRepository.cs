namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Assessments.ValueObjects;
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

    public async Task<Assessment?> GetAssessmentById(
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

    public async Task<List<Assessment>> GetAssessmentsForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Assessment>()
            .Where(a => a.Students.Any(s => s.StudentId == studentId))
            .ToListAsync(cancellationToken);

    public async Task<List<Assessment>> GetCurrentAssessmentsForStudent(
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
    
    public async Task<Provision?> GetProvisionById(
        ProvisionId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Provision>()
            .FirstOrDefaultAsync(provision => provision.Id == id, cancellationToken);

    public async Task<List<Provision>> GetProvisions(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Provision>()
            .ToListAsync(cancellationToken);

    public async Task<bool> DoesProvisionCodeExist(
        ProvisionCode code,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Provision>()
            .AnyAsync(provision => provision.Code == code, cancellationToken);


    public async Task<StudentProvision?> GetStudentProvisionById(
        StudentProvisionId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentProvision>()
            .FirstOrDefaultAsync(provision => provision.Id == id, cancellationToken);

    public async Task<List<StudentProvision>> GetStudentProvisionsFromCurrentYear(
        CancellationToken cancellationToken = default)
    {
        int currentYear = DateTimeOffset.UtcNow.Year;

        return await _context
            .Set<StudentProvision>()
            .Where(provision => provision.Year == currentYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StudentProvision>> GetStudentProvisions(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentProvision>()
            .ToListAsync(cancellationToken);

    public async Task<bool> DoesCurrentStudentProvisionExist(
        StudentId studentId,
        ProvisionId provisionId,
        int year,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentProvision>()
            .AnyAsync(provision =>
                    provision.StudentId == studentId
                    && provision.ProvisionId == provisionId
                    && provision.Year == year
                    && !provision.IsDeleted,
                cancellationToken);

    public void Insert(Assessment assessment) => _context.Set<Assessment>().Add(assessment);
    public void Insert(Provision provision) => _context.Set<Provision>().Add(provision);
    public void Insert(StudentProvision studentProvision) => _context.Set<StudentProvision>().Add(studentProvision);

}