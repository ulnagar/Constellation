namespace Constellation.Application.Training.Modules.GetUploadedTrainingCertificateFileById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Errors;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Shared;
using Core.Models.Attachments;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetUploadedTrainingCertificateFileByIdQueryHandler
    : IQueryHandler<GetUploadedTrainingCertificateFileByIdQuery, CompletionRecordCertificateDetailsDto>
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;

    public GetUploadedTrainingCertificateFileByIdQueryHandler(
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService)
    {
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
    }

    public async Task<Result<CompletionRecordCertificateDetailsDto>> Handle(GetUploadedTrainingCertificateFileByIdQuery request, CancellationToken cancellationToken)
    {
        Attachment attachment = await _attachmentRepository.GetTrainingCertificateByLinkId(request.LinkId, cancellationToken);

        if (attachment is null)
            return Result.Failure<CompletionRecordCertificateDetailsDto>(DomainErrors.Documents.TrainingCertificate.NotFound);

        Result<AttachmentResponse> attempt =
            await _attachmentService.GetAttachmentFile(AttachmentType.TrainingCertificate, request.LinkId,
                cancellationToken);

        if (attempt.IsFailure)
        {
            return Result.Failure<CompletionRecordCertificateDetailsDto>(attempt.Error);
        }

        return new CompletionRecordCertificateDetailsDto
        {
            Id = attachment.Id,
            Name = attachment.Name,
            FileData = attempt.Value.FileData,
            FileType = attempt.Value.FileType,
            FileDataBase64 = Convert.ToBase64String(attempt.Value.FileData)
        };
    }
}
