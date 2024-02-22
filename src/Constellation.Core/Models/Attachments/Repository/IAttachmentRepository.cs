#nullable enable
namespace Constellation.Core.Models.Attachments.Repository;

using ValueObjects;
using Attachments;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAttachmentRepository
{
    Task<List<Attachment>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Attachment>> GetSubsetOverSizeInDb(int maxSize, int count, CancellationToken cancellationToken = default);
    Task<Attachment?> GetByTypeAndLinkId(AttachmentType type, string linkId, CancellationToken cancellationToken = default);
    Task<Attachment?> GetTrainingCertificateByLinkId(string linkId, CancellationToken cancellationToken = default);
    Task<Attachment?> GetAcademicReportByLinkId(string linkId, CancellationToken cancellationToken = default);
    Task<bool> DoesAwardCertificateExistInDatabase(string linkId, CancellationToken cancellationToken = default);

    void Insert(Attachment file);
    void Remove(Attachment file);
}
