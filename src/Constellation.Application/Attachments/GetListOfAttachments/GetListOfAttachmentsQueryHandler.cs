namespace Constellation.Application.Attachments.GetListOfAttachments;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Attachments;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfAttachmentsQueryHandler 
    : IQueryHandler<GetListOfAttachmentsQuery, List<Attachment>>
{
    private readonly IAttachmentRepository _attachmentRepository;

    public GetListOfAttachmentsQueryHandler(IAttachmentRepository attachmentRepository)
    {
        _attachmentRepository = attachmentRepository;
    }

    public async Task<Result<List<Attachment>>> Handle(GetListOfAttachmentsQuery request, CancellationToken cancellationToken)
    {
        List<Attachment> files = await _attachmentRepository.GetAll(cancellationToken);

        return files;
    }
}