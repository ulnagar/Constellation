using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Constellation.Infrastructure.GatewayConfigurations
{
    public class CanvasGatewayConfiguration : ICanvasGatewayConfiguration, ITransientService
    {
        public CanvasGatewayConfiguration(IConfiguration configuration)
        {
            ApiEndpoint = configuration["AppSettings:CanvasGateway:ServerUrl"];
            ApiKey = configuration["AppSettings:CanvasGateway:Key"];
        }

        public string ApiEndpoint { get; set; }
        public string ApiKey { get; set; }
    }
}
