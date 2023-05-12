namespace Constellation.Infrastructure.HangfireServer;

using Serilog;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;

    public Worker(ILogger logger)
    {
        _logger = logger.ForContext<BackgroundService>();
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
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
        _logger.Information("Stopping Hangfire Background Service");

        return base.StopAsync(cancellationToken);
    }
}
