namespace Constellation.Infrastructure.HangfireServer;

using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.HangfireServer.Services;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Hangfire;
using Hangfire.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration config)
	{
        services.AddScoped<ICurrentUserService, CurrentUserService>();

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

        return services;
    }

}
