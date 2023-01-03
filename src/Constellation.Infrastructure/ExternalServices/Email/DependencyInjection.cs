namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.Email;

public static class EmailServicesRegistration
{
    public static IServiceCollection AddEmailExternalService(this IServiceCollection services)
    {
        services.AddScoped<IEmailGatewayConfiguration, Configuration>();
        services.AddScoped<IEmailGateway, Gateway>();
        services.AddScoped<IEmailService, Service>();

        return services;
    }
}