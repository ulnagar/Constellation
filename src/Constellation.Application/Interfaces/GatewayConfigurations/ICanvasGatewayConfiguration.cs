using System.Collections.Generic;

namespace Constellation.Application.Interfaces.GatewayConfigurations
{
    public interface ICanvasGatewayConfiguration
    {
        string ApiEndpoint { get; set; }
        string ApiKey { get; set; }
    }
}
