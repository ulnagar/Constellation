using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Constellation.Infrastructure.HangfireServer;
public static class DependencyInjection
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration config)
	{
        services.AddHangfireServerInfrastructureComponents(config);

        services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddHangfire((provider, configuration) => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(config.GetConnectionString("Hangfire"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

        services.AddHangfireServer(opts =>
            {
                opts.ServerName = "Constellation.Infrastructure.HangfireServer";
            });

        services.AddControllersWithViews();

        var diagnosticSource = new DiagnosticListener("Constellation.Infrastructure.HangfireServer");
        services.AddSingleton<DiagnosticSource>(diagnosticSource);
        services.AddSingleton<DiagnosticListener>(diagnosticSource);

        return services;
    }

}
