namespace Constellation.Application.Training.Modules.GetUploadedTrainingCertificationMetadata;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetUploadedTrainingCertificateFileMetadataHandler
    : IQueryHandler<GetUploadedTrainingCertificateMetadataQuery, CompletionRecordCertificateDto>
{
    private readonly IAttachmentRepository _attachmentRepository;

    public GetUploadedTrainingCertificateFileMetadataHandler(
        IAttachmentRepository attachmentRepository)
    {
        _attachmentRepository = attachmentRepository;
    }

    public async Task<Result<CompletionRecordCertificateDto>> Handle(GetUploadedTrainingCertificateMetadataQuery request, CancellationToken cancellationToken)
    {
        var file = await _attachmentRepository.GetTrainingCertificateByLinkId(request.LinkId, cancellationToken);

        return new CompletionRecordCertificateDto
        {
            Id = file.Id,
            Name = file.Name,
            FileType = file.FileType
        };
    }
}
