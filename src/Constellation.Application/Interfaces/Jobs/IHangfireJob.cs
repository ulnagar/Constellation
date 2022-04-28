using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IHangfireJob
    {
        Task StartJob(Guid jobId, CancellationToken token);
    }
}
