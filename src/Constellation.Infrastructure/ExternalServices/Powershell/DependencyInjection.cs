namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways.PowershellGateway;
using Constellation.Infrastructure.ExternalServices.Powershell;
using Microsoft.Extensions.Configuration;

public static class PowershellServicesRegistration
{
    public static IServiceCollection AddPowershellExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PowershellGatewayConfiguration>();
        services.Configure<PowershellGatewayConfiguration>(configuration.GetSection(PowershellGatewayConfiguration.Section));

        services.AddScoped<IPowershellGateway, Gateway>();

        return services;
    }
}