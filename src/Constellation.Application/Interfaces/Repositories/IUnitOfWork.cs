namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Primitives;
using System.Threading;
using System.Threading.Tasks;

public interface IUnitOfWork
{
    Task AddIntegrationEvent(IIntegrationEvent integrationEvent);
    Task CompleteAsync(CancellationToken token);
    Task CompleteAsync();
}
