namespace Constellation.Core.Models.Attachments.Services;

using DTOs;
using Shared;
using System.Threading;
using System.Threading.Tasks;
using ValueObjects;

public interface IAttachmentService
{
    Task<Result<AttachmentResponse>> GetAttachmentFile(AttachmentType type, string linkId, CancellationToken cancellationToken = default);
    Task<Result> StoreAttachmentData(Attachment attachment, byte[] fileData, bool overwrite = false, CancellationToken cancellationToken = default);
    Task<Result> RemediateEntry(Attachment attachment, CancellationToken cancellationToken = default);
    void DeleteAttachment(Attachment attachment);
}
