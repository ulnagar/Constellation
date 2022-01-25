using System.Collections.Generic;

namespace Constellation.Application.Interfaces.GatewayConfigurations
{
    public interface ISentralGatewayConfiguration
    {
        string Username { get; set; }
        string Password { get; set; }
        string Server { get; set; }
        string ContactPreference { get; set; }
        IDictionary<string, string> XPaths { get; set; }

        static class ContactPreferenceOptions
        {
            public const string MotherFirstThenFather = "MotherThenFather";
            public const string FatherFirstThenMother = "FatherThenMother";
            public const string BothParentsIfPresent = "Both";
        }
    }
}
