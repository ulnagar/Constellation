namespace Constellation.Application.Interfaces.Configuration;

public class SentralGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Sentral";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
    public string ApiTenant { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    
    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
            return false;

        if (string.IsNullOrWhiteSpace(Username))
            return false;

        if (string.IsNullOrWhiteSpace(Password))
            return false;

        return true;
    }
}
