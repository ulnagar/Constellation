﻿namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificateFileById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetUploadedTrainingCertificateFileByIdQueryHandler
    : IQueryHandler<GetUploadedTrainingCertificateFileByIdQuery, CompletionRecordCertificateDetailsDto>
{
    private readonly IStoredFileRepository _storedFileRepository;

    public GetUploadedTrainingCertificateFileByIdQueryHandler(
        IStoredFileRepository storedFileRepository)
    {
        _storedFileRepository = storedFileRepository;
    }

    public async Task<Result<CompletionRecordCertificateDetailsDto>> Handle(GetUploadedTrainingCertificateFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await _storedFileRepository.GetTrainingCertificateByLinkId(request.LinkId, cancellationToken);

        return new CompletionRecordCertificateDetailsDto
        {
            Id = file.Id,
            Name = file.Name,
            FileData = file.FileData,
            FileType = file.FileType,
            FileDataBase64 = Convert.ToBase64String(file.FileData)
        };
    }
}