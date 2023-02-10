namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Threading;
using System.Threading.Tasks;

public interface ICasualService
{
    Task<ServiceOperationResult<Casual>> CreateCasual(CasualDto casualResource);
    Task<ServiceOperationResult<Casual>> UpdateCasual(int casualId, CasualDto casualResource);
    Task RemoveCasual(int casualId, CancellationToken cancellationToken = default);
}
