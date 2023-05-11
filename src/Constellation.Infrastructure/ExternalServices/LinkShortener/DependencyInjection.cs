namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.LinkShortener;
using Microsoft.Extensions.Configuration;

public static class LinkShortenerServicesRegistration
{
    public static IServiceCollection AddLinkShortenerExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LinkShortenerGatewayConfiguration>();
        services.Configure<LinkShortenerGatewayConfiguration>(configuration.GetSection("AppSettings:LinkShortener"));

        services.AddScoped<ILinkShortenerGateway, Gateway>();

        return services;
    }
}