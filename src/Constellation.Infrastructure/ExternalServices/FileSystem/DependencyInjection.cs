namespace Microsoft.Extensions.DependencyInjection;

using Configuration;
using Constellation.Application.Interfaces.Configuration;

public static class FileSystemServicesRegistration
{
    public static IServiceCollection AddFileSystemService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FileSystemGatewayConfiguration>();
        services.Configure<FileSystemGatewayConfiguration>(configuration.GetSection(FileSystemGatewayConfiguration.Section));
        
        return services;
    }
}