using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Setup and import appsettings.json
var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true);

var config = builder.Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddHangfireServerInfrastructureComponents(config)
            .AddHangfire((provider, configuration) => configuration
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
                }))
            .AddHangfireServer(opts =>
            {
                opts.ServerName = "Constellation.Infrastructure.HangfireServer";
            })
            .AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>())
    .Build();

host.Start();

Console.ReadKey();

// Setup and start Hangfire Server
//GlobalConfiguration.Configuration.UseSqlServerStorage(config.GetConnectionString("Hangfire"));

//using (var server = new BackgroundJobServer(new BackgroundJobServerOptions { ServerName = "Constellation.Infrastructure.Hangfireserver" }))
//{
//    Console.WriteLine("Hangfire Server started. Press any key to exit...");
//    Console.ReadKey();
//}
