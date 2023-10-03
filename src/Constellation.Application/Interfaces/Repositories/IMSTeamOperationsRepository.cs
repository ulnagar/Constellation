namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IMSTeamOperationsRepository
{
    Task<MSTeamOperationsList> ToProcess();
    Task<MSTeamOperationsList> OverdueToProcess();
    Task<MSTeamOperation> ForMarkingCompleteOrCancelled(int id);
    Task<List<MSTeamOperation>> GetByCoverId(ClassCoverId coverId, CancellationToken cancellationToken = default);
    void Insert(MSTeamOperation operation);
}