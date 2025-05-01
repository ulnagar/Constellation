namespace Constellation.Application.Domains.Training.Queries.GetUploadedTrainingCertificationMetadata;

using Abstractions.Messaging;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetUploadedTrainingCertificateMetadataHandler
    : IQueryHandler<GetUploadedTrainingCertificateMetadataQuery, CompletionRecordCertificateDto>
{
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger _logger;

    public GetUploadedTrainingCertificateMetadataHandler(
        IAttachmentService attachmentService,
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _logger = logger.ForContext<GetUploadedTrainingCertificateMetadataHandler>();
    }

    public async Task<Result<CompletionRecordCertificateDto>> Handle(GetUploadedTrainingCertificateMetadataQuery request, CancellationToken cancellationToken)
    {
        Result<AttachmentResponse> fileRequest = await _attachmentService.GetAttachmentFile(AttachmentType.TrainingCertificate, request.LinkId, cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(GetUploadedTrainingCertificateMetadataHandler), request, true)
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to retrieve file from store");

            return Result.Failure<CompletionRecordCertificateDto>(fileRequest.Error);
        }

        return new CompletionRecordCertificateDto
        {
            Name = fileRequest.Value.FileName,
            FileType = fileRequest.Value.FileType
        };
    }
}
