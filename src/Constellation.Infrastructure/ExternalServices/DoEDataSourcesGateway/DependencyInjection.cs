namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.CESE;
using Microsoft.Extensions.Configuration;

public static class DoEDataServicesGatewayRegistration
{
    public static IServiceCollection AddDoEDataServicesGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DoEDataSourcesGatewayConfiguration>();
        services.Configure<DoEDataSourcesGatewayConfiguration>(configuration.GetSection(DoEDataSourcesGatewayConfiguration.Section));

        services.AddScoped<IDoEDataSourcesGateway, Gateway>();

        return services;
    }
}