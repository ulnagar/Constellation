namespace Constellation.Infrastructure.HangfireServer;

using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.HangfireServer.Services;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;
using Constellation.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Scrutor;

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

        // Search for an register all the Repository classes that are located at
        // Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
        services.Scan(selector =>
            selector.FromAssemblies(
                Constellation.Application.AssemblyReference.Assembly,
                Constellation.Infrastructure.AssemblyReference.Assembly)
            .AddClasses(classes => classes.InNamespaceOf<UnitOfWork>(), false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        // Search for and register all the Services classes that are located at
        // Constellation.Infrastructure.Services
        services.Scan(selector =>
            selector.FromAssemblies(
                Constellation.Application.AssemblyReference.Assembly,
                Constellation.Infrastructure.AssemblyReference.Assembly)
            .AddClasses(classes => classes.InNamespaceOf<AuthService>(), false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }

}
