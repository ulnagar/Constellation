using System.Collections.Generic;

namespace Constellation.Application.Interfaces.GatewayConfigurations
{
    public interface IAdobeConnectGatewayConfiguration
    {
        string Url { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string BaseFolder { get; set; }
        IDictionary<string, string> Groups { get; set; }
        string TemplateSco { get; set; }
    }
}
