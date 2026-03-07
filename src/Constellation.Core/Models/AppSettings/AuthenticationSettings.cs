namespace Constellation.Core.Models.AppSettings;

public sealed class AuthenticationSettings
{
    private AuthenticationSettings() { }
    
    public AuthenticationSettings(
        bool loginEnabled,
        bool ssoEnabled)
    {
        LoginEnabled = loginEnabled;
        SSOEnabled = ssoEnabled;
    }
    
    public bool LoginEnabled { get; set; }
    public bool SSOEnabled { get; set; }
}