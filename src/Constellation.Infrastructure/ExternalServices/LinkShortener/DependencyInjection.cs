namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.LinkShortener;

public static class LinkShortenerServicesRegistration
{
    public static IServiceCollection AddLinkShortenerExternalService(this IServiceCollection services)
    {
        services.AddScoped<ILinkShortenerGatewayConfiguration, Configuration>();
        services.AddScoped<ILinkShortenerGateway, Gateway>();

        return services;
    }
}