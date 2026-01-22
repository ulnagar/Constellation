namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;

public sealed class JobDispatcherService<T> : IJobDispatcherService<T> where T : IHangfireJob
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger _logger;
    private readonly T _service;

    public JobDispatcherService(
        T service,
        ILogger logger)
    {
        _service = service;
        _logger = logger
            .ForContext<T>();
    }

    public async Task StartJob(CancellationToken token)
    {
        Guid jobId = Guid.NewGuid();

        if (_semaphore.CurrentCount == 0)
        {
            _logger
                .ForContext(nameof(IHangfireJob), typeof(T).Name)
                .ForContext("JobId", jobId)
                .Information("Attempt to start job failed due to no free locks");

            return;
        }

        _logger
            .ForContext(nameof(IHangfireJob), typeof(T).Name)
            .ForContext("JobId", jobId)
            .Information("Attempt to start job waiting for available lock");

        bool solo = await _semaphore.WaitAsync(0, token);
        if (!solo)
        {
            _logger
                .ForContext(nameof(IHangfireJob), typeof(T).Name)
                .ForContext("JobId", jobId)
                .Information("Available lock not found for job indicating it is already running");

            return;
        }

        _logger
            .ForContext(nameof(IHangfireJob), typeof(T).Name)
            .ForContext("JobId", jobId)
            .Information("Available lock found and taken for job");

        if (!token.IsCancellationRequested)
        {
            _logger
                .ForContext(nameof(IHangfireJob), typeof(T).Name)
                .ForContext("JobId", jobId)
                .Information("Starting job");
            try
            {
                await _service.StartJob(jobId, token);

                if (token.IsCancellationRequested)
                {
                    _logger
                        .ForContext(nameof(IHangfireJob), typeof(T).Name)
                        .ForContext("JobId", jobId)
                        .Warning("Job cancelled");
                }
                else
                {
                    _logger
                        .ForContext(nameof(IHangfireJob), typeof(T).Name)
                        .ForContext("JobId", jobId)
                        .Information("Job finished");
                }
            }
            catch (Exception e)
            {
                _logger
                    .ForContext(nameof(IHangfireJob), typeof(T).Name)
                    .ForContext("JobId", jobId)
                    .ForContext(nameof(Exception), e, true)
                    .Error("Job failed with exception");
            }
        }

        _logger
            .ForContext(nameof(IHangfireJob), typeof(T).Name)
            .ForContext("JobId", jobId)
            .Information("Releasing lock taken for job");

        _semaphore.Release();
    }
}