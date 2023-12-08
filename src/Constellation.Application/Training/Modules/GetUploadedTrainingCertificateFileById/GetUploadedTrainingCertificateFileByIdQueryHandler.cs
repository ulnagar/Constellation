namespace Constellation.Application.Training.Modules.GetUploadedTrainingCertificateFileById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Shared;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetUploadedTrainingCertificateFileByIdQueryHandler
    : IQueryHandler<GetUploadedTrainingCertificateFileByIdQuery, CompletionRecordCertificateDetailsDto>
{
    private readonly IAttachmentService _attachmentService;

    public GetUploadedTrainingCertificateFileByIdQueryHandler(
        IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    public async Task<Result<CompletionRecordCertificateDetailsDto>> Handle(GetUploadedTrainingCertificateFileByIdQuery request, CancellationToken cancellationToken)
    {
        Result<AttachmentResponse> attempt = await _attachmentService.GetAttachmentFile(AttachmentType.TrainingCertificate, request.LinkId, cancellationToken);

        if (attempt.IsFailure)
            return Result.Failure<CompletionRecordCertificateDetailsDto>(attempt.Error);

        return new CompletionRecordCertificateDetailsDto
        {
            Name = attempt.Value.FileName,
            FileData = attempt.Value.FileData,
            FileType = attempt.Value.FileType,
            FileDataBase64 = Convert.ToBase64String(attempt.Value.FileData)
        };
    }
}
