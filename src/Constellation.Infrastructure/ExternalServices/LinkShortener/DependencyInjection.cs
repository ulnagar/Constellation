namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.LinkShortener;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class LinkShortenerServicesRegistration
{
    public static IServiceCollection AddLinkShortenerExternalService(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<LinkShortenerGatewayConfiguration>();
        services.Configure<LinkShortenerGatewayConfiguration>(configuration.GetSection(LinkShortenerGatewayConfiguration.Section));

        if (!environment.IsProduction())
            services.AddScoped<ILinkShortenerGateway, LogOnlyGateway>();
        else
            services.AddScoped<ILinkShortenerGateway, Gateway>();

        return services;
    }
}