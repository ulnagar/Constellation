﻿namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.Canvas;
using Microsoft.Extensions.Configuration;

public static class CanvasServicesRegistration
{
    public static IServiceCollection AddCanvasExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CanvasGatewayConfiguration>();
        services.Configure<CanvasGatewayConfiguration>(configuration.GetSection(CanvasGatewayConfiguration.Section));

        services.AddScoped<ICanvasGateway, Gateway>();

        return services;
    }
}