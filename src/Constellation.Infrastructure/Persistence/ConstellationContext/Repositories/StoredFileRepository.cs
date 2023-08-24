namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;

internal class StoredFileRepository : IStoredFileRepository
{
    private readonly AppDbContext _context;

    public StoredFileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StoredFile>> GetAwardCertificatesFromList(
        List<string> linkIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StoredFile>()
            .Where(file => 
                file.LinkType == StoredFile.AwardCertificate &&
                linkIds.Contains(file.LinkId))
            .ToListAsync(cancellationToken);

    public async Task<List<StoredFile>> GetTrainingCertificatesFromList(
        List<string> recordIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StoredFile>()
            .Where(file =>
                file.LinkType == StoredFile.TrainingCertificate &&
                recordIds.Contains(file.LinkId))
            .ToListAsync(cancellationToken);

    public async Task<StoredFile?> GetTrainingCertificateByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StoredFile>()
            .Where(file =>
                file.LinkType == StoredFile.TrainingCertificate &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<StoredFile?> GetAssignmentSubmissionByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StoredFile>()
            .Where(file =>
                file.LinkType == StoredFile.CanvasAssignmentSubmission &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<StoredFile?> GetAcademicReportByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StoredFile>()
            .Where(file =>
                file.LinkType == StoredFile.StudentReport &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> DoesAwardCertificateExistInDatabase(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StoredFile>()
            .AnyAsync(file =>
                file.LinkType == StoredFile.AwardCertificate &&
                file.LinkId == linkId,
                cancellationToken);

    public async Task<StoredFile?> GetAwardCertificateByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StoredFile>()
            .Where(file =>
                file.LinkType == StoredFile.AwardCertificate &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(StoredFile file) =>
        _context.Set<StoredFile>().Add(file);
}
