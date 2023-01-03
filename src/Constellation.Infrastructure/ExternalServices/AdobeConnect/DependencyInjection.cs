namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.AdobeConnect;
using Microsoft.Extensions.Configuration;

public static class AdobeConnectServicesRegistration
{
    public static IServiceCollection AddAdobeConnectExternalService(this IServiceCollection services)
    {
        services.AddScoped<IAdobeConnectGatewayConfiguration, Configuration>();
        services.AddScoped<IAdobeConnectGateway, Gateway>();
        services.AddScoped<IAdobeConnectService, Service>();

        return services;
    }
}