namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.NetworkStatistics;

public static class NetworkStatisticsServicesRegistration
{
    public static IServiceCollection AddNetworkStatisticsExternalService(this IServiceCollection services)
    {
        services.AddScoped<INetworkStatisticsGatewayConfiguration, Configuration>();
        services.AddScoped<INetworkStatisticsGateway, Gateway>();

        return services;
    }
}