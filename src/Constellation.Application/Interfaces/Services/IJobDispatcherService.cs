using Constellation.Application.Interfaces.Jobs;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IJobDispatcherService<T> where T : IHangfireJob
    {
        Task StartJob(CancellationToken token);
    }
}
