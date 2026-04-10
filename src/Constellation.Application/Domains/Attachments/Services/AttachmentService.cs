namespace Constellation.Application.Domains.Attachments.Services;

using Core.Errors;
using Core.Models.Attachments;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Enums;
using Core.Models.Attachments.Errors;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Shared;
using Interfaces.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly FileSystemGatewayConfiguration _configuration;
    private readonly ILogger _logger;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IOptions<FileSystemGatewayConfiguration> configuration,
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
        Attachment? record = await _attachmentRepository.GetByTypeAndLinkId(type, linkId, cancellationToken);
        if (record is null)
        {
            _logger
                .ForContext(nameof(GetAttachmentFile), new { Type = type, LinkId = linkId}, true)
                .ForContext(nameof(Error), AttachmentErrors.NotFound(type, linkId), true)
                .Warning("Failed to retrieve attachment file");

            return Result.Failure<AttachmentResponse>(AttachmentErrors.NotFound(type, linkId));
        }

        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            if (record.FileData is null)
            {
                _logger
                    .ForContext(nameof(GetAttachmentFile), new { Type = type, LinkId = linkId }, true)
                    .ForContext(nameof(Error), AttachmentErrors.NotFound(type, linkId), true)
                    .Warning("Failed to retrieve attachment file");

                return Result.Failure<AttachmentResponse>(AttachmentErrors.NotFound(type, linkId));
            }

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
        bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        bool useDisk = _configuration.IsConfigured();

        using SHA256 sha = SHA256.Create();
        byte[] checksum = sha.ComputeHash(fileData);

        if (!string.IsNullOrWhiteSpace(attachment.Checksum))
        {
            if (attachment.Checksum != Convert.ToHexString(checksum).Replace("-", string.Empty, StringComparison.InvariantCultureIgnoreCase) && !overwrite)
                return Result.Failure(AttachmentErrors.FileDataExists);

            if (attachment.Checksum == Convert.ToHexString(checksum).Replace("-", string.Empty, StringComparison.InvariantCultureIgnoreCase))
                return Result.Success();
        }

        if (useDisk && fileData.Length > _configuration.MaxDbStoreSize)
        {
            string basePath = _configuration.BaseFilePath;

            // Get file extension
            string extension = attachment.FileType switch
            {
                MediaTypeNames.Application.Pdf => "pdf",
                MediaTypeNames.Image.Jpeg => "jpg",
                _ => "txt"
            };

            // Store file on disk
            string filePath = attachment.LinkType == AttachmentType.TempFile ?
                $"{basePath}/{attachment.LinkType.Value}/{attachment.Id}.{extension}" :
                $"{basePath}/{attachment.LinkType.Value}/{attachment.LinkId[..2]}/{attachment.LinkId}.{extension}";

            Result attempt = attachment.AttachPath(filePath, fileData.Length, Convert.ToHexString(checksum).Replace("-", string.Empty, StringComparison.InvariantCultureIgnoreCase), overwrite);

            if (attempt.IsFailure)
                return attempt;

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllBytesAsync(filePath, fileData, cancellationToken);

            return Result.Success();
        }
        else
        {
            // Store file in database
            Result attempt = attachment.AttachData(fileData, Convert.ToHexString(checksum).Replace("-", string.Empty, StringComparison.InvariantCultureIgnoreCase), overwrite);

            return attempt;
        }
    }
    
    public async Task<Result> RemediateEntry(
        Attachment attachment,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(attachment.FilePath) && attachment.FileData != Array.Empty<byte>())
            return Result.Failure(new("Invalid Attachment", "Attachment is not eligible for remediation"));
        
        if (!_configuration.IsConfigured())
            return Result.Failure(ApplicationErrors.InvalidConfiguration(nameof(FileSystemGatewayConfiguration)));

        string basePath = _configuration.BaseFilePath;

        // Get file extension
        string extension = attachment.FileType switch
        {
            MediaTypeNames.Application.Pdf => "pdf",
            MediaTypeNames.Image.Jpeg => "jpg",
            _ => "txt"
        };

        // Check file exists on disk
        string filePath = $"{basePath}/{attachment.LinkType.Value}/{attachment.LinkId[..2]}/{attachment.LinkId}.{extension}";

        if (Path.Exists(filePath))
        {
            byte[] fileContents = await File.ReadAllBytesAsync(filePath, cancellationToken);
            int fileSize = fileContents.Length;

            using SHA256 sha = SHA256.Create();
            byte[] checksum = sha.ComputeHash(fileContents);

            Result attempt = attachment.AttachPath(filePath, fileSize, Convert.ToHexString(checksum).Replace("-", string.Empty, StringComparison.InvariantCultureIgnoreCase), true);
            
            return attempt;
        }

        return Result.Failure(new("Invalid File", "Could not locate Attachment on disk"));
    }

    public void DeleteAttachment(Attachment attachment)
    {
        if (!string.IsNullOrWhiteSpace(attachment.FilePath))
        {
            if (File.Exists(attachment.FilePath))
                File.Delete(attachment.FilePath);
        }

        _attachmentRepository.Remove(attachment);
    }
}
