namespace Constellation.Application.Interfaces.GatewayConfigurations
{
    public interface IEmailGatewayConfiguration
    {
        string Server { get; set; }
        int Port { get; set; }
        string Username { get; set; }
        string Password { get; set; }
    }
}
