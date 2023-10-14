namespace Constellation.Application.Attachments.Services;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Attachments.Errors;
using Core.Models.Attachments;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using Interfaces.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _logger;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _attachmentRepository = attachmentRepository;
        _configuration = configuration.Value;
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

    public async Task<Result> StoreAttachmentData(
        Attachment attachment, 
        byte[] fileData,
        CancellationToken cancellationToken = default)
    {
        string basePath = _configuration.Attachments.BaseFilePath;
        int maxDbSize = _configuration.Attachments.MaxDBStoreSize;

        if (fileData.Length > maxDbSize)
        {
            // Get file extension
            string extension = attachment.FileType switch
            {
                MediaTypeNames.Application.Pdf => "pdf",
                MediaTypeNames.Image.Jpeg => "jpg",
                _ => "txt"
            };

            // Store file on disk
            string filePath = $"{basePath}/{attachment.LinkType.Value}/{attachment.LinkId[..2]}/{attachment.LinkId}.{extension}";

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await File.WriteAllBytesAsync(filePath, fileData, cancellationToken);

            Result attempt = attachment.AttachPath(filePath, fileData.Length);

            return attempt;
        }
        else
        {
            // Store file in database
            Result attempt = attachment.AttachData(fileData, true);

            return attempt;
        }
    }

    public void DeleteAttachment(Attachment attachment)
    {
        if (attachment.FilePath is not null)
        {
            if (File.Exists(attachment.FilePath))
                File.Delete(attachment.FilePath);
        }

        _attachmentRepository.Remove(attachment);
    }
}
