namespace Constellation.Application.Attachments.Services;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Attachments.Errors;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using GetAttachmentFile;
using Serilog;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ILogger _logger;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        ILogger logger)
    {
        _attachmentRepository = attachmentRepository;
        _logger = logger.ForContext<IAttachmentService>();
    }

    public async Task<Result<AttachmentResponse>> GetAttachmentFile(
    AttachmentType type,
    string linkId,
    CancellationToken cancellationToken = default)
    {
        Attachment record = await _attachmentRepository.GetByTypeAndLinkId(type, linkId, cancellationToken);
        if (record is null)
        {
            _logger
                .ForContext(nameof(GetAttachmentFile), new { Type = type, LinkId = linkId}, true)
                .ForContext(nameof(Error), AttachmentErrors.NotFound(type, linkId), true)
                .Warning("Failed to retrieve attachment file");

            return Result.Failure<AttachmentResponse>(AttachmentErrors.NotFound(type, linkId));
        }

        if (record.FilePath is null)
        {
            return new AttachmentResponse(
                record.FileType,
                record.Name,
            record.FileData);
        }

        if (!File.Exists(record.FilePath))
        {
            _logger
                .ForContext(nameof(GetAttachmentFile), new { Type = type, LinkId = linkId}, true)
                .ForContext(nameof(Error), AttachmentErrors.NotFoundOnDisk(type, linkId), true)
            .Warning("Failed to retrieve attachment file");

            return Result.Failure<AttachmentResponse>(AttachmentErrors.NotFoundOnDisk(type, linkId));
        }

        byte[] fileData = await File.ReadAllBytesAsync(record.FilePath, cancellationToken);

        return new AttachmentResponse(
            record.FileType,
            record.Name,
            fileData);
    }
}
