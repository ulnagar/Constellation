namespace Constellation.Core.Models.AppSettings;

public sealed class AuthenticationSettings
{
    private AuthenticationSettings() { }

    public AuthenticationSettings(
        bool loginEnabled,
        bool ssoEnabled)
    {
        LoginEnabled = loginEnabled;
        SsoEnabled = ssoEnabled;
    }

    public bool LoginEnabled { get; private set; }
    public bool SsoEnabled { get; private set; }
}