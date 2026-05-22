namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.Canvas;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;

public static class CanvasServicesRegistration
{
    public static IServiceCollection AddCanvasExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection configurationValues = configuration.GetSection(CanvasGatewayConfiguration.Section);

        services.AddOptions<CanvasGatewayConfiguration>();
        services.Configure<CanvasGatewayConfiguration>(configurationValues);

        services.AddHttpClient<ICanvasGateway, Gateway>(client =>
            {
                string apiEndpoint = configurationValues["ApiEndpoint"];
                string apiKey = configurationValues["ApiKey"];

                if (string.IsNullOrWhiteSpace(apiEndpoint) || string.IsNullOrWhiteSpace(apiKey))
                    return;

                client.BaseAddress = new Uri(apiEndpoint);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                CookieContainer = new CookieContainer(),
                UseProxy = true,
                Proxy = WebRequest.DefaultWebProxy
            });
        
        services.AddHttpClient("CanvasFileUpload")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                UseProxy = true,
                Proxy = WebRequest.DefaultWebProxy
            });

        return services;
    }
}