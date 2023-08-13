namespace Constellation.Application.Interfaces.Configuration;

using System.Collections.Generic;

public class SentralGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Sentral";

    public string Username { get; set; }
    public string Password { get; set; }
    public string ServerUrl { get; set; }
    public ContactPreferenceOptions ContactPreference { get; set; }
    public Dictionary<string, string> XPaths { get; set; }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
            return false;

        if (string.IsNullOrWhiteSpace(Username))
            return false;

        if (string.IsNullOrWhiteSpace(Password))
            return false;

        return true;
    }

    public enum ContactPreferenceOptions
    {
        MotherThenFather,
        FatherThenMother,
        Both
    }
}
