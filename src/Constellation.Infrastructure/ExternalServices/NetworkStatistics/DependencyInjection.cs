namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.NetworkStatistics;
using Microsoft.Extensions.Configuration;

public static class NetworkStatisticsServicesRegistration
{
    public static IServiceCollection AddNetworkStatisticsExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NetworkStatisticsGatewayConfiguration>();
        services.Configure<NetworkStatisticsGatewayConfiguration>(configuration.GetSection("AppSettings:NetworkStatistics"));

        services.AddScoped<INetworkStatisticsGateway, Gateway>();

        return services;
    }
}