namespace Constellation.Infrastructure.ExternalServices.CESE;

public sealed class CESEGatewayConfiguration
{
    public string DataPath { get; set; }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(DataPath))
            return false;

        return true;
    }
}
