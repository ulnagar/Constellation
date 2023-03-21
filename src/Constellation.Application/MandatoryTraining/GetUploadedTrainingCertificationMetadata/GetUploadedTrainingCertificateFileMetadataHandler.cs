namespace Constellation.Application.MandatoryTraining.GetUploadedTrainingCertificationMetadata;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetUploadedTrainingCertificateFileMetadataHandler
    : IQueryHandler<GetUploadedTrainingCertificateMetadataQuery, CompletionRecordCertificateDto>
{
    private readonly IStoredFileRepository _storedFileRepository;

    public GetUploadedTrainingCertificateFileMetadataHandler(
        IStoredFileRepository storedFileRepository)
    {
        _storedFileRepository = storedFileRepository;
    }

    public async Task<Result<CompletionRecordCertificateDto>> Handle(GetUploadedTrainingCertificateMetadataQuery request, CancellationToken cancellationToken)
    {
        var file = await _storedFileRepository.GetTrainingCertificateByLinkId(request.LinkId, cancellationToken);

        return new CompletionRecordCertificateDto
        {
            Id = file.Id,
            Name = file.Name,
            FileType = file.FileType
        };
    }
}
