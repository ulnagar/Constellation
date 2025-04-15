namespace Constellation.Infrastructure.ExternalServices.Powershell;

public class PowershellGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Powershell";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) 
            return false;
        
        return true;
    }
}
