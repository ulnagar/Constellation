﻿namespace Constellation.Application.Interfaces.Configuration;

using Constellation.Core.Enums;
using System.Collections.Generic;

public sealed class TeamsGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Teams";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            return false;

        return true;
    }

    public List<string> MandatoryOwnerIds { get; set; }
    public List<string> StudentTeamOwnerIds { get; set; }
    public Dictionary<Grade, List<string>> StudentTeamChannelOwnerIds { get; set; }

}