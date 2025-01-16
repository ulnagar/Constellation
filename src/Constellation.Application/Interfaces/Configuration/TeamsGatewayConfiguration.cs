namespace Constellation.Application.Interfaces.Configuration;

using System.Collections.Generic;

public sealed class TeamsGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Teams";
    public List<string> MandatoryOwnerIds { get; set; }
}