using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Filters;

namespace Constellation.Infrastructure.DependencyInjection
{
    public static class LoggingConfiguration
    {
        public static void SetupLogging(IConfiguration configuration)
        {
            var seqServer = configuration["AppSettings:LoggingServer:ServerUrl"];
            var seqKey = configuration["AppSettings:LoggingServer:ApiKey"];

            //Log.Logger = 

            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/ClassMonitor.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Day);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IClassMonitorJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/PermissionUpdate.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Day);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IPermissionUpdateJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/LessonNotifications.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Day);
                    config.Filter.ByIncludingOnly(Matching.FromSource<ILessonNotificationsJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/AttendanceReports.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IAttendanceReportJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/AbsenceMonitor.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IAbsenceMonitorJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/RollMarkingReport.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IRollMarkingReportJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/SchoolRegister.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<ISchoolRegisterJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/UserManager.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<IUserManagerJob>());
                })
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
                    config.WriteTo.File("logs/TrackItSync.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<ITrackItSyncJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/FamilyDetailsSync.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<ISentralFamilyDetailsSyncJob>());
                })
                .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/StudentReportSync.log",
                        Serilog.Events.LogEventLevel.Information,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Month);
                    config.Filter.ByIncludingOnly(Matching.FromSource<ISentralReportSyncJob>());
                })
               .WriteTo.Logger(config =>
                {
                    config.WriteTo.File("logs/System.log",
                        Serilog.Events.LogEventLevel.Warning,
                        outputTemplate: "[{Timestamp} {Level:u3} {SourceContext}] {Message}\n{Exception}",
                        rollingInterval: RollingInterval.Day);
                    config.Filter.ByExcluding(Matching.FromSource<IClassMonitorJob>());
                    config.Filter.ByExcluding(Matching.FromSource<IPermissionUpdateJob>());
                    config.Filter.ByExcluding(Matching.FromSource<ILessonNotificationsJob>());
                    config.Filter.ByExcluding(Matching.FromSource<IAttendanceReportJob>());
                    config.Filter.ByExcluding(Matching.FromSource<IAbsenceMonitorJob>());
                    config.Filter.ByExcluding(Matching.FromSource<IRollMarkingReportJob>());
                    config.Filter.ByExcluding(Matching.FromSource<ISchoolRegisterJob>());
                    config.Filter.ByExcluding(Matching.FromSource<IUserManagerJob>());
                    config.Filter.ByExcluding(Matching.FromSource<IEmailGateway>());
                    config.Filter.ByExcluding(Matching.FromSource<IAuthService>());
                    config.Filter.ByExcluding(Matching.FromSource<ILinkShortenerGateway>());
                    config.Filter.ByExcluding(Matching.FromSource<ISMSGateway>());
                    config.Filter.ByExcluding(Matching.FromSource<ITrackItSyncJob>());
                    config.Filter.ByExcluding(Matching.FromSource<ISentralFamilyDetailsSyncJob>());
                    config.Filter.ByExcluding(Matching.FromSource<ISentralReportSyncJob>());
                });

            if (!string.IsNullOrWhiteSpace(seqServer) && !string.IsNullOrWhiteSpace(seqKey))
            {
                logger.WriteTo.Logger(config =>
                {
                    config.WriteTo.Seq(seqServer, apiKey: seqKey);
                });
            }

            Log.Logger = logger.CreateLogger();
        }
    }
}
