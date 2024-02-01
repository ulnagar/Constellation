namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Infrastructure.ExternalServices.LissServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class LissServerServicesRegistration
{
    public static IServiceCollection AddLissServer(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        //services.AddOptions<LinkShortenerGatewayConfiguration>();
        //services.Configure<LinkShortenerGatewayConfiguration>(configuration.GetSection(LinkShortenerGatewayConfiguration.Section));

        //if (!environment.IsProduction())
        //    services.AddScoped<ILissServerService, LissServerService>();
        //else
        //    services.AddScoped<ILissServerService, LissServerService>();

        services.AddScoped<ILissServerService, LissServerService>();
        
        return services;
    }
}