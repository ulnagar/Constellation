namespace Constellation.Application.Interfaces.GatewayConfigurations
{
    public interface ILinkShortenerGatewayConfiguration
    {
        string ApiEndpoint { get; set; }
        string ApiKey { get; set; }
    }
}
