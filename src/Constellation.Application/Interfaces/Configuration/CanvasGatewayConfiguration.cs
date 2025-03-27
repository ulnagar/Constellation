namespace Constellation.Application.Interfaces.Configuration;

using System.Collections.Generic;

public sealed class CanvasGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Canvas";

    public string ApiEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public bool UseGroups { get; set; }
    public bool UseSections { get; set; }

    public List<string> CourseAdmins { get; set; } = [];

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(ApiEndpoint))
            return false;

        if (string.IsNullOrWhiteSpace(ApiKey)) 
            return false;

        return true;
    }
}