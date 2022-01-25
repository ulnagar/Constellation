using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Constellation.Infrastructure.GatewayConfigurations
{
    public class LinkShortenerGatewayConfiguration : ILinkShortenerGatewayConfiguration, ITransientService
    {
        public LinkShortenerGatewayConfiguration(IConfiguration configuration)
        {
            ApiEndpoint = configuration["AppSettings:LinkShortenerGateway:ServerUrl"];
            ApiKey = configuration["AppSettings:LinkShortenerGateway:Key"];
        }

        public string ApiEndpoint { get; set; }
        public string ApiKey { get; set; }
    }
}
