﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Attachments.Repository;
using Core.Models.Attachments;
using Core.Models.Attachments.Identifiers;
using Core.Models.Attachments.ValueObjects;
using Microsoft.EntityFrameworkCore;

internal class AttachmentRepository : IAttachmentRepository
{
    private readonly AppDbContext _context;

    public AttachmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Attachment>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .ToListAsync(cancellationToken);

    public async Task<List<Attachment>> GetSubsetOverSizeInDb(
        int maxSize,
        int count,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(attachment =>
                attachment.FileData != null &&
                attachment.FileSize > maxSize)
            .OrderBy(attachment => attachment.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<Attachment?> GetById(
        AttachmentId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .SingleOrDefaultAsync(file => file.Id == id, cancellationToken);

    public async Task<Attachment?> GetByTypeAndLinkId(
        AttachmentType type,
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(attachment => 
                attachment.LinkType == type &&
                attachment.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Attachment>> GetAwardCertificatesFromList(
        List<string> linkIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(file => 
                file.LinkType == AttachmentType.AwardCertificate &&
                linkIds.Contains(file.LinkId))
            .ToListAsync(cancellationToken);

    public async Task<List<Attachment>> GetTrainingCertificatesFromList(
        List<string> recordIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(file =>
                file.LinkType == AttachmentType.TrainingCertificate &&
                recordIds.Contains(file.LinkId))
            .ToListAsync(cancellationToken);

    public async Task<Attachment?> GetTrainingCertificateByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(file =>
                file.LinkType == AttachmentType.TrainingCertificate &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Attachment?> GetAssignmentSubmissionByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(file =>
                file.LinkType == AttachmentType.CanvasAssignmentSubmission &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Attachment?> GetAcademicReportByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(file =>
                file.LinkType == AttachmentType.StudentReport &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> DoesAwardCertificateExistInDatabase(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .AnyAsync(file =>
                file.LinkType == AttachmentType.AwardCertificate &&
                file.LinkId == linkId,
                cancellationToken);

    public async Task<Attachment?> GetAwardCertificateByLinkId(
        string linkId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Attachment>()
            .Where(file =>
                file.LinkType == AttachmentType.AwardCertificate &&
                file.LinkId == linkId)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(Attachment file) =>
        _context.Set<Attachment>().Add(file);

    public void Remove(Attachment file) =>
        _context.Set<Attachment>().Remove(file);
}
