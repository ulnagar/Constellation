namespace Constellation.Infrastructure.ExternalServices.LinkShortener;

public class LinkShortenerGatewayConfiguration
{
    public string ApiEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(ApiEndpoint))
            return false;

        if (string.IsNullOrWhiteSpace(ApiKey)) 
            return false;

        return true;
    }
}
