namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.CESE;
using Microsoft.Extensions.Configuration;

public static class CeseServicesRegistration
{
    public static IServiceCollection AddCeseExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CESEGatewayConfiguration>();
        services.Configure<CESEGatewayConfiguration>(configuration.GetSection("AppSettings:CESE"));

        services.AddScoped<ICeseGateway, Gateway>();

        return services;
    }
}