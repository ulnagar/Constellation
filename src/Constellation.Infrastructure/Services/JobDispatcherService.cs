using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    public class JobDispatcherService<T> : IJobDispatcherService<T>, IScopedService where T : IHangfireJob
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<T> _logger;
        private readonly T _service;

        public JobDispatcherService(ILogger<T> logger, T service)
        {
            _logger = logger;
            _service = service;
        }

        public async Task StartJob(CancellationToken token)
        {
            var jobId = Guid.NewGuid();

            if (_semaphore.CurrentCount == 0)
            {
                _logger.LogInformation("Attempt to start job {job} ({id}) failed due to no free locks", typeof(T).Name, jobId);

                return;
            }

            _logger.LogInformation("Attempt to start job {job} ({id}) waiting for available lock", typeof(T).Name, jobId);
            await _semaphore.WaitAsync(token);
            _logger.LogInformation("Available lock found and taken for job {job} ({id})", typeof(T).Name, jobId);

            if (!token.IsCancellationRequested)
            {
                _logger.LogInformation("Starting job {job} ({id})", typeof(T).Name, jobId);
                try
                {
                    await _service.StartJob(jobId, token);
                    _logger.LogInformation("Job {job} ({id}) finished", typeof(T).Name, jobId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Job {job} ({id}) failed with exception {e}", typeof(T).Name, jobId, e.Message);
                }
            }

            _logger.LogInformation("Releasing lock taken for job {job} ({id})", typeof(T).Name, jobId);
            _semaphore.Release();
        }
    }
}
