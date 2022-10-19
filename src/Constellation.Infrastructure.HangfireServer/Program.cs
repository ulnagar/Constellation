using Constellation.Infrastructure.HangfireServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// Setup and import appsettings.json
var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true);

var config = builder.Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.RegisterServices(config))
    .Build();

host.Start();

Console.ReadKey();