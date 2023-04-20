namespace Constellation.Infrastructure.HangfireServer;

using Hangfire;
using Hangfire.SqlServer;

public class Worker : BackgroundService
{
    private readonly Serilog.ILogger _logger;
    private BackgroundJobServer? _server;

    public Worker(
        Serilog.ILogger logger,
        IConfiguration config)
    {
        _logger = logger.ForContext<BackgroundService>();

        //GlobalConfiguration.Configuration
        //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        //    .UseSimpleAssemblyNameTypeSerializer()
        //    .UseRecommendedSerializerSettings()
        //    .UseSqlServerStorage(
        //        config.GetConnectionString("Hangfire"),
        //        new SqlServerStorageOptions
        //        {
        //            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        //            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        //            QueuePollInterval = TimeSpan.Zero,
        //            UseRecommendedIsolationLevel = true,
        //            DisableGlobalLocks = true
        //        });
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        //_server = new BackgroundJobServer(new BackgroundJobServerOptions
        //{
        //    ServerName = "Hangfire Service"
        //});

        _logger.Information("Starting Hangfire Background Service");

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        //_server?.Dispose();

        _logger.Information("Stopping Hangfire Background Service");

        return base.StopAsync(cancellationToken);
    }
}
