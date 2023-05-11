namespace Constellation.Infrastructure.ExternalServices.NetworkStatistics;

public class NetworkStatisticsGatewayConfiguration
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return false;

        if (string.IsNullOrWhiteSpace(Key))
            return false;

        return true;
    }
}
