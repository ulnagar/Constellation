namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Configuration;
using Application.Interfaces.Jobs;
using Application.Interfaces.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Core.Abstractions.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Shared;
using Microsoft.Extensions.Options;

internal sealed class AttachmentManagementJob : IAttachmentManagementJob
{
    private readonly AppConfiguration.AttachmentsConfiguration _configuration;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AttachmentManagementJob(
        IOptions<AppConfiguration> configuration,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _configuration = configuration.Value.Attachments;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<IAttachmentManagementJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        List<Attachment> attachments = await _attachmentRepository.GetSubsetOverSizeInDb(_configuration.MaxDBStoreSize, 10, cancellationToken);

        foreach (Attachment attachment in attachments)
        {
            _logger
                .Information("Processing file {filename} from attachment {id}", attachment.Name, attachment.Id.Value);

            Result attempt = await _attachmentService.StoreAttachmentData(attachment, attachment.FileData, true, cancellationToken);

            if (attempt.IsFailure)
            {
                _logger
                    .Warning("Failed to move file {filename} from Database to Disk", attachment.Name);
            }
            else
            {
                _logger
                    .ForContext(nameof(Attachment.FilePath), attachment.FilePath)
                    .Information("Successfully moved file {filename} from Database to Disk", attachment.Name);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}