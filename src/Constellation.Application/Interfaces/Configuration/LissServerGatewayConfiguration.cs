namespace Constellation.Application.Interfaces.Configuration;

public sealed class LissServerGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:LissServer";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            return false;

        return true;
    }
}