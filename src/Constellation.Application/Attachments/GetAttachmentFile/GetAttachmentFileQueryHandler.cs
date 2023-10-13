namespace Constellation.Application.Attachments.GetAttachmentFile;

using Abstractions.Messaging;
using Core.Models.Attachments.Services;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttachmentFileQueryHandler
    : IQueryHandler<GetAttachmentFileQuery, AttachmentResponse>
{
    private readonly IAttachmentService _attachmentService;

    public GetAttachmentFileQueryHandler(
        IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    public async Task<Result<AttachmentResponse>> Handle(
        GetAttachmentFileQuery request,
        CancellationToken cancellationToken) => 
        await _attachmentService.GetAttachmentFile(request.Type, request.LinkId, cancellationToken);
}