namespace Constellation.Infrastructure.ExternalServices.Canvas;

public sealed class CanvasGatewayConfiguration
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
