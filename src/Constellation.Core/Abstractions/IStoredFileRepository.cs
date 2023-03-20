namespace Constellation.Core.Abstractions;

using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStoredFileRepository
{
    Task<List<StoredFile>> GetTrainingCertificatesFromList(List<string> recordIds, CancellationToken cancellationToken = default);
    Task<StoredFile?> GetTrainingCertificateByLinkId(string linkId, CancellationToken cancellationToken = default);
}
