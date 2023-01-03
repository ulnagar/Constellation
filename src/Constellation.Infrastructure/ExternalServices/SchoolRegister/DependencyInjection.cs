namespace Microsoft.Extensions.DependencyInjection;

using Constellation.Application.Interfaces.Gateways;
using Constellation.Infrastructure.ExternalServices.SchoolRegister;

public static class SchoolRegisterServicesRegistration
{
    public static IServiceCollection AddSchoolRegisterExternalService(this IServiceCollection services)
    {
        services.AddScoped<ISchoolRegisterGateway, Gateway>();

        return services;
    }
}