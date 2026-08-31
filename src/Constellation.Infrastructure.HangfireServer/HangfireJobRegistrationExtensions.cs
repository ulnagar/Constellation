namespace Constellation.Infrastructure.HangfireServer;

using Constellation.Infrastructure.Caches.AuthenticatorMetadata;
using Hangfire;
using Hangfire.Storage;
using Jobs;

public static class HangfireJobRegistrationExtensions
{
    public static async Task RegisterSystemJobsWithHangfire(this WebApplication app)
    {
        JobStorage.Current = app.Services.GetRequiredService<JobStorage>();

        if (app.Environment.IsDevelopment())
            ClearAllJobs();

        using (var scope = app.Services.CreateScope())
        {
            scope.ServiceProvider
                .GetRequiredService<IAuthenticatorMetadataLoader>()
                .Load();
        }
    }

    private static void ClearAllJobs()
    {
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var client = new BackgroundJobClient(JobStorage.Current);

        // Delete recurring jobs first since they will keep re-enqueuing
        var recurringJobs = JobStorage.Current
            .GetConnection()
            .GetRecurringJobs();

        foreach (var job in recurringJobs)
        {
            RecurringJob.RemoveIfExists(job.Id);
        }

        // Delete all enqueued jobs across every queue
        var queues = monitoring.Queues();
        foreach (var queue in queues)
        {
            var count = (int)monitoring.EnqueuedCount(queue.Name);
            var jobs = monitoring.EnqueuedJobs(queue.Name, 0, count);
            foreach (var job in jobs)
            {
                client.Delete(job.Key);
            }
        }

        // Delete scheduled (delayed) jobs
        var scheduledCount = (int)monitoring.ScheduledCount();
        var scheduledJobs = monitoring.ScheduledJobs(0, scheduledCount);
        foreach (var job in scheduledJobs)
        {
            client.Delete(job.Key);
        }

        // Delete processing jobs
        var processingCount = (int)monitoring.ProcessingCount();
        var processingJobs = monitoring.ProcessingJobs(0, processingCount);
        foreach (var job in processingJobs)
        {
            client.Delete(job.Key);
        }

        // Delete failed jobs
        var failedCount = (int)monitoring.FailedCount();
        var failedJobs = monitoring.FailedJobs(0, failedCount);
        foreach (var job in failedJobs)
        {
            client.Delete(job.Key);
        }
    }
}