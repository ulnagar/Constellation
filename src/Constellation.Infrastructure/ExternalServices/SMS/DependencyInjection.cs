namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.SMS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class SmsServicesRegistration
{
    public static IServiceCollection AddSmsExternalService(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<SMSGatewayConfiguration>();
        services.Configure<SMSGatewayConfiguration>(configuration.GetSection(SMSGatewayConfiguration.Section));

        services.AddScoped<ISMSGateway, Gateway>();

        services.AddScoped<ISMSService, Service>();

        return services;
    }
}