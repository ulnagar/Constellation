namespace Constellation.Application.Interfaces.Configuration;

public sealed class FileSystemGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:FileSystem";

    public string BaseFilePath { get; set; } = string.Empty;
    public int MaxDbStoreSize { get; set; }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(BaseFilePath))
            return false;

        return true;
    }
}