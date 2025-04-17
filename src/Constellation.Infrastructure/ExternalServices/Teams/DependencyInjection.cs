namespace Constellation.Infrastructure.ExternalServices.Teams;

using Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways.TeamsGateway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class TeamsServicesRegistration
{
    public static IServiceCollection AddTeamsExternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TeamsGatewayConfiguration>();
        services.Configure<TeamsGatewayConfiguration>(configuration.GetSection(TeamsGatewayConfiguration.Section));

        services.AddScoped<ITeamsGateway, Gateway>();

        return services;
    }
}