namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.SMS;
using Microsoft.Extensions.Configuration;

public static class SMSServicesRegistration
{
    public static IServiceCollection AddSMSExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SMSGatewayConfiguration>();
        services.Configure<SMSGatewayConfiguration>(configuration.GetSection(SMSGatewayConfiguration.Section));

        services.AddScoped<ISMSGateway, Gateway>();
        services.AddScoped<ISMSService, Service>();

        return services;
    }
}