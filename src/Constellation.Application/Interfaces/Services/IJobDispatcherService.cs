namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Interfaces.Jobs;
using System.Threading;
using System.Threading.Tasks;

public interface IJobDispatcherService<T> where T : IHangfireJob
{
    Task StartJob(CancellationToken token);
}