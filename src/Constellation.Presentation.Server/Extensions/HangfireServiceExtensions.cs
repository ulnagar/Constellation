namespace Constellation.Presentation.Server.Extensions;

using Constellation.Infrastructure.Jobs;
using Constellation.Presentation.Server.Infrastructure;
using Hangfire;
using Hangfire.SqlServer;

public static class HangfireServiceExtensions
{
    public static IServiceCollection AddConstellationHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire((provider, config) => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                configuration.GetConnectionString("Hangfire"),
                new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    EnableHeavyMigrations = true,
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

        services.AddTransient<HangfireAuthorizationFilter>();

        return services;
    }
    public static async Task RegisterSystemJobsWithHangfire(this WebApplication app)
    {
        //RecurringJob.AddOrUpdate<MdsRefreshJob>(
        //    recurringJobId: "mds-refresh",
        //    methodCall: job => job.RefreshAsync(JobCancellationToken.Null),
        //    cronExpression: Cron.Weekly());

        using (var scope = app.Services.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<MdsRefreshJob>();
            await job.InitialiseAsync();
        }
    }
}
