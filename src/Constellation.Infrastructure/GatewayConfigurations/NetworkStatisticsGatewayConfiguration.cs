using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Constellation.Infrastructure.GatewayConfigurations
{
    public class NetworkStatisticsGatewayConfiguration : INetworkStatisticsGatewayConfiguration, ITransientService
    {
        public NetworkStatisticsGatewayConfiguration(IConfiguration configuration)
        {
            Url = configuration["AppSettings:NetworkStatisticsGateway:ServerUrl"];
            Key = configuration["AppSettings:NetworkStatisticsGateway:Key"];
        }

        public string Url { get; set; }
        public string Key { get; set; }
    }
}
