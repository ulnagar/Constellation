namespace Constellation.Infrastructure.ExternalServices.DoEDataSourcesGateway;

public sealed class DoEDataSourcesGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:DoEDataServices";
    public string CESEDataPath { get; set; }
    public string DataCollectionsDataPath { get; set; }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(CESEDataPath))
            return false;

        if (string.IsNullOrWhiteSpace(DataCollectionsDataPath))
            return false;

        return true;
    }
}
