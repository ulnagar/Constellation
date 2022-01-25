using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Constellation.Infrastructure.GatewayConfigurations
{
    public class SMSGatewayConfiguration : ISMSGatewayConfiguration, ITransientService
    {
        public SMSGatewayConfiguration(IConfiguration configuration)
        {
            Key = configuration["AppSettings:SMSGateway:Key"];
            Secret = configuration["AppSettings:SMSGateway:Secret"];
        }

        public string Key { get; set; }
        public string Secret { get; set; }
    }
}
