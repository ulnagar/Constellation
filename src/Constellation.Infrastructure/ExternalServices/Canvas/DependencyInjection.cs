namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.Canvas;

public static class CanvasServicesRegistration
{
    public static IServiceCollection AddCanvasExternalService(this IServiceCollection services)
    {
        services.AddScoped<ICanvasGatewayConfiguration, Configuration>();
        services.AddScoped<ICanvasGateway, Gateway>();

        return services;
    }
}