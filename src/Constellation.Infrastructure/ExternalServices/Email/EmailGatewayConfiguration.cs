namespace Constellation.Infrastructure.ExternalServices.Email;

public class EmailGatewayConfiguration
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(Server)) 
            return false;

        if (Port == 0)
            return false;
        
        return true;
    }
}
