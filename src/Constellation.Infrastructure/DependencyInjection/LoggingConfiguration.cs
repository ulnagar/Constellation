namespace Constellation.Infrastructure.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Filters;

public static class LoggingConfiguration
{
    public static void SetupLogging(IConfiguration configuration, LogEventLevel minimumFileLogLevel)
    {
        string seqServer = configuration["Constellation:LoggingServer:ServerUrl"];
        string seqKey = configuration["Constellation:LoggingServer:ApiKey"];

        LoggerConfiguration logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Logger(config =>
            {
                config.WriteTo.File("logs/EmailGateway.log",
                    Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                    rollingInterval: RollingInterval.Month);
                config.Filter.ByIncludingOnly(Matching.FromSource<IEmailGateway>());
            })
            .WriteTo.Logger(config =>
            {
                config.WriteTo.File("logs/Authentication.log",
                    Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                    rollingInterval: RollingInterval.Day);
                config.Filter.ByIncludingOnly(Matching.FromSource<IAuthService>());
            })
            .WriteTo.Logger(config =>
            {
                config.WriteTo.File("logs/LinkShortenerGateway.log",
                    Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                    rollingInterval: RollingInterval.Day);
                config.Filter.ByIncludingOnly(Matching.FromSource<ILinkShortenerGateway>());
            })
            .WriteTo.Logger(config =>
            {
                config.WriteTo.File("logs/SMSGateway.log",
                    Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                    rollingInterval: RollingInterval.Day);
                config.Filter.ByIncludingOnly(Matching.FromSource<ISMSGateway>());
            })
           .WriteTo.Logger(config =>
            {
                config.WriteTo.File("logs/System.log",
                    minimumFileLogLevel,
                    outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                    rollingInterval: RollingInterval.Day);
            });

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
