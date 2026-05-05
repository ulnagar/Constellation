namespace Constellation.Core.Models.Hosting.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IHostingRepository
{
    Task<List<Newsletter>> GetAllNewsletters(CancellationToken cancellationToken = default);
    Task<Newsletter?> GetNewsletterByIssue(int issue, CancellationToken cancellationToken = default);

    Task<List<Livestream>> GetAllLivestreams(CancellationToken cancellationToken = default);
    Task<Livestream?> GetLivestreamById(Guid id, CancellationToken cancellationToken = default);

    void Insert(Newsletter newsletter);
    void Insert(Livestream livestream);
}
