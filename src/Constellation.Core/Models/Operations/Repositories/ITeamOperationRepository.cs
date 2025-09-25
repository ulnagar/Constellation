namespace Constellation.Core.Models.Operations.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITeamOperationRepository
{
    Task<TeamOperation> GetById(int id, CancellationToken cancellationToken = default);
    Task<List<TeamOperation>> GetDue(CancellationToken cancellationToken = default);
    Task<List<TeamOperation>> GetOverdue(CancellationToken cancellationToken = default);
    void Insert(TeamOperation operation);

}