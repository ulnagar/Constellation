namespace Constellation.Application.Awards.GetAwardCertificate;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAwardCertificateQueryHandler
    : IQueryHandler<GetAwardCertificateQuery, StoredFile>
{
    private readonly IStoredFileRepository _fileRepository;

    public GetAwardCertificateQueryHandler(
        IStoredFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<Result<StoredFile>> Handle(GetAwardCertificateQuery request, CancellationToken cancellationToken)
    {
        var file = await _fileRepository.GetAwardCertificateByLinkId(request.AwardId.ToString(), cancellationToken);

        if (file is null)
            return Result.Failure<StoredFile>(DomainErrors.Documents.AwardCertificate.NotFound(request.AwardId));

        return file;
    }
}
