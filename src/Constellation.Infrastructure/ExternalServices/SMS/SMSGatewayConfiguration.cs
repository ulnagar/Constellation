namespace Constellation.Infrastructure.ExternalServices.SMS;

public class SMSGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:SMS";

    public string Host { get; set; }
    public int Port { get; set; }
    public string Version { get; set; }
    public string Key { get; set; }
    public string Secret { get; set; }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(Host))
            return false;

        if (Port == 0)
            return false;

        if (string.IsNullOrWhiteSpace(Key))
            return false;

        if (string.IsNullOrWhiteSpace(Secret)) 
            return false;

        return true;
    }
}
