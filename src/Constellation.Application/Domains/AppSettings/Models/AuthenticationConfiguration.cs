namespace Constellation.Application.Domains.AppSettings.Models;

public sealed record AuthenticationConfiguration
{
    public AuthenticationConfiguration(
        bool loginEnabled,
        bool ssoEnabled)
    {
        LoginEnabled = loginEnabled;
        SSOEnabled = ssoEnabled;
    }
    
    public bool LoginEnabled { get; init; }
    public bool SSOEnabled { get; init; }
}