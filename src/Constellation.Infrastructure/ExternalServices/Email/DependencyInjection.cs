namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.Email;
using Microsoft.Extensions.Configuration;

public static class EmailServicesRegistration
{
    public static IServiceCollection AddEmailExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailGatewayConfiguration>();
        services.Configure<EmailGatewayConfiguration>(configuration.GetSection(EmailGatewayConfiguration.Section));

        services.AddScoped<IEmailGateway, Gateway>();
        services.AddScoped<IEmailService, Service>();

        return services;
    }
}