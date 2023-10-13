namespace Constellation.Core.Models.Attachments.Services;

using Application.Attachments.GetAttachmentFile;
using Shared;
using System.Threading;
using System.Threading.Tasks;
using ValueObjects;

public interface IAttachmentService
{
    Task<Result<AttachmentResponse>> GetAttachmentFile(AttachmentType type, string linkId, CancellationToken cancellationToken = default);

}
