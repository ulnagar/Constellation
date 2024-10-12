namespace Constellation.Application.Interfaces.Repositories;

using System.Threading;
using System.Threading.Tasks;

public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken token);
    Task CompleteAsync();
}
