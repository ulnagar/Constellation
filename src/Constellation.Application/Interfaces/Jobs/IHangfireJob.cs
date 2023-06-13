namespace Constellation.Application.Interfaces.Jobs;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IHangfireJob
{
    Task StartJob(Guid jobId, CancellationToken cancellationToken);
}
