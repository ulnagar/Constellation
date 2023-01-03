using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Constellation.Infrastructure.ExternalServices.LinkShortener
{
    public class Configuration : ILinkShortenerGatewayConfiguration, ITransientService
    {
        public Configuration(IConfiguration configuration)
        {
            ApiEndpoint = configuration["AppSettings:LinkShortenerGateway:ServerUrl"];
            ApiKey = configuration["AppSettings:LinkShortenerGateway:Key"];
        }

        public string ApiEndpoint { get; set; }
        public string ApiKey { get; set; }
    }
}
