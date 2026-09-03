namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Constellation.Core.Models.Students.ValueObjects;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Microsoft.EntityFrameworkCore;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

internal sealed class EnrolmentApplicationRepository
: IEnrolmentApplicationRepository
{
    private readonly EnrolmentDbContext _context;

    public EnrolmentApplicationRepository(
        EnrolmentDbContext context)
    {
        _context = context;
    }

    public async Task<List<Application>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .ToListAsync(cancellationToken);

    public async Task<Application?> GetApplicationById(
        ApplicationId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .FirstOrDefaultAsync(
                application => application.Id == id,
                cancellationToken);


    public async Task<Application?> GetApplicationByReference(
        EnrolmentPeriodId periodId,
        string reference,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .FirstOrDefaultAsync(entry =>
                    entry.PeriodId == periodId &&
                    entry.ApplicationReference == reference,
                cancellationToken);

    public async Task<Application?> GetApplicationBySRN(
        EnrolmentPeriodId periodId,
        StudentReferenceNumber studentReferenceNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .FirstOrDefaultAsync(entry =>
                    entry.PeriodId == periodId &&
                    entry.StudentReferenceNumber == studentReferenceNumber,
                cancellationToken);

    public async Task<List<Application>> GetApplicationByStudentName(
        EnrolmentPeriodId periodId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(entry =>
                    entry.PeriodId == periodId &&
                    entry.StudentName.FirstName == firstName &&
                    entry.StudentName.LastName == lastName)
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetApplicationsByPeriod(
        EnrolmentPeriodId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(entry => entry.PeriodId == id)
            .ToListAsync(cancellationToken);

    public async Task<List<Application>> GetListFromIds(
        List<ApplicationId> ids,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Application>()
            .Where(application => ids.Contains(application.Id))
            .ToListAsync(cancellationToken);

    public void Insert(Application application) => _context.Set<Application>().Add(application);
}