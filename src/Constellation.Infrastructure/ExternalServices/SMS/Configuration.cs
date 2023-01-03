using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Constellation.Infrastructure.ExternalServices.SMS
{
    public class Configuration : ISMSGatewayConfiguration, ITransientService
    {
        public Configuration(IConfiguration configuration)
        {
            Key = configuration["AppSettings:SMSGateway:Key"];
            Secret = configuration["AppSettings:SMSGateway:Secret"];
        }

        public string Key { get; set; }
        public string Secret { get; set; }
    }
}
