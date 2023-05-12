namespace Constellation.Infrastructure.ExternalServices.AdobeConnect;

using System.Collections.Generic;

public sealed class AdobeConnectGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:AdobeConnect";
    
    public string ServerUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BaseFolder { get; set; } = string.Empty;
    public Dictionary<string, string> Groups { get; set; } = new();
    public string TemplateSco { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
            return false;

        if (string.IsNullOrWhiteSpace(Username)) 
            return false;

        if (string.IsNullOrWhiteSpace(Password))
            return false;

        if (string.IsNullOrWhiteSpace(BaseFolder))
            return false;

        if (string.IsNullOrWhiteSpace(TemplateSco))
            return false;

        if (!Groups.Any())
            return false;

        return true;
    }
}
