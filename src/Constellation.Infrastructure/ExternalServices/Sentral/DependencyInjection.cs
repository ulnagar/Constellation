namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.Sentral;

public static class SentralServicesRegistration
{
    public static IServiceCollection AddSentralExternalService(this IServiceCollection services)
    { 
        services.AddScoped<ISentralGatewayConfiguration, Configuration>();
        services.AddScoped<ISentralGateway, Gateway>();
        services.AddScoped<ISentralService, Service>();

        return services;
    }
}