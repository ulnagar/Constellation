namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.SMS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class SMSServicesRegistration
{
    public static IServiceCollection AddSMSExternalService(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<SMSGatewayConfiguration>();
        services.Configure<SMSGatewayConfiguration>(configuration.GetSection(SMSGatewayConfiguration.Section));

        if (!environment.IsDevelopment())
            services.AddScoped<ISMSGateway, LogOnlyGateway>();
        else
            services.AddScoped<ISMSGateway, Gateway>();

        services.AddScoped<ISMSService, Service>();

        return services;
    }
}