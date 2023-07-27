namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.ExternalServices.Sentral;
using Microsoft.Extensions.Configuration;
using System.Net;

public static class SentralServicesRegistration
{
    public static IServiceCollection AddSentralExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("sentral")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                CookieContainer = new CookieContainer(),
                UseProxy = true,
                Proxy = WebRequest.DefaultWebProxy
            });

        services.AddOptions<SentralGatewayConfiguration>();
        services.Configure<SentralGatewayConfiguration>(configuration.GetSection(SentralGatewayConfiguration.Section));

        services.AddScoped<ISentralGateway, Gateway>();
        services.AddScoped<ISentralService, Service>();

        return services;
    }
}