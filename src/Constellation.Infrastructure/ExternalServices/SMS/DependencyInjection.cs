namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.SMS;

public static class SMSServicesRegistration
{
    public static IServiceCollection AddSMSExternalService(this IServiceCollection services)
    { 
        services.AddScoped<ISMSGatewayConfiguration, Configuration>();
        services.AddScoped<ISMSGateway, Gateway>();
        services.AddScoped<ISMSService, Service>();

        return services;
    }
}