namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Awards.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAwardNominationRepository
{
    Task<List<NominationPeriod>> GetAll(CancellationToken cancellationToken = default);
    Task<List<NominationPeriod>> GetCurrentAndFuture(CancellationToken cancellationToken = default);
    Task<NominationPeriod> GetById(AwardNominationPeriodId periodId, CancellationToken cancellationToken = default);
    void Insert(NominationPeriod period);
}
