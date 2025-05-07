namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways.LissServerGateway;
using Constellation.Infrastructure.ExternalServices.LissServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class LissServerServicesRegistration
{
    public static IServiceCollection AddLissServer(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<LissServerGatewayConfiguration>();
        services.Configure<LissServerGatewayConfiguration>(configuration.GetSection(LissServerGatewayConfiguration.Section));

        services.AddScoped<ILissServerGateway, LissServerGateway>();
        
        return services;
    }
}