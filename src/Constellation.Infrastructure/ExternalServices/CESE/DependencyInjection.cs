namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.CESE;

public static class CeseServicesRegistration
{
    public static IServiceCollection AddCeseExternalService(this IServiceCollection services)
    {
        services.AddScoped<ICeseGateway, Gateway>();

        return services;
    }
}