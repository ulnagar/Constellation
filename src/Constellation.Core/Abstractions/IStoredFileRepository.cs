#nullable enable
namespace Constellation.Core.Abstractions;

using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStoredFileRepository
{
    Task<List<StoredFile>> GetTrainingCertificatesFromList(List<string> recordIds, CancellationToken cancellationToken = default);
    Task<StoredFile?> GetTrainingCertificateByLinkId(string linkId, CancellationToken cancellationToken = default);

    Task<StoredFile?> GetAssignmentSubmissionByLinkId(string linkId, CancellationToken cancellationToken = default);

    Task<StoredFile?> GetAcademicReportByLinkId(string linkId, CancellationToken cancellationToken = default);

    Task<StoredFile?> GetAwardCertificateByLinkId(string linkId, CancellationToken cancellationToken = default);
    Task<List<StoredFile>> GetAwardCertificatesFromList(List<string> linkIds, CancellationToken cancellationToken = default);
    Task<bool> DoesAwardCertificateExistInDatabase(string linkId, CancellationToken cancellationToken = default);

    void Insert(StoredFile file);
}
