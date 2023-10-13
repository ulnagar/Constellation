#nullable enable

namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Attachments.ValueObjects;
using Models.Attachments;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAttachmentRepository
{
    Task<Attachment?> GetById(int id, CancellationToken cancellationToken = default);
    Task<List<Attachment>> GetAll(CancellationToken cancellationToken = default);
    Task<Attachment?> GetByTypeAndLinkId(AttachmentType type, string linkId, CancellationToken cancellationToken = default);

    Task<List<Attachment>> GetTrainingCertificatesFromList(List<string> recordIds, CancellationToken cancellationToken = default);
    Task<Attachment?> GetTrainingCertificateByLinkId(string linkId, CancellationToken cancellationToken = default);

    Task<Attachment?> GetAssignmentSubmissionByLinkId(string linkId, CancellationToken cancellationToken = default);

    Task<Attachment?> GetAcademicReportByLinkId(string linkId, CancellationToken cancellationToken = default);

    Task<Attachment?> GetAwardCertificateByLinkId(string linkId, CancellationToken cancellationToken = default);
    Task<List<Attachment>> GetAwardCertificatesFromList(List<string> linkIds, CancellationToken cancellationToken = default);
    Task<bool> DoesAwardCertificateExistInDatabase(string linkId, CancellationToken cancellationToken = default);

    void Insert(Attachment file);
}
