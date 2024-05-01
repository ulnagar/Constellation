namespace Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

public static class LoggingConfiguration
{
    public static void SetupLogging(IConfiguration configuration, LogEventLevel minimumFileLogLevel)
    {
        string seqServer = configuration["Constellation:LoggingServer:ServerUrl"];
        string seqKey = configuration["Constellation:LoggingServer:ApiKey"];

        LoggerConfiguration logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext();

        if (!string.IsNullOrWhiteSpace(seqServer))
        {
            if (!string.IsNullOrWhiteSpace(seqKey))
            {
                logger.WriteTo.Logger(config =>
                {
                    config.WriteTo.Seq(seqServer, apiKey: seqKey);
                });
            }
            else
            {
                logger.WriteTo.Logger(config =>
                {
                    config.WriteTo.Seq(seqServer);
                });
            }
        }

        Log.Logger = logger.CreateLogger();
    }
}
