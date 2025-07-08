namespace Constellation.Application.Interfaces.Configuration;

using Constellation.Core.Enums;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using System.Collections.Generic;

public sealed class TeamsGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Teams";

    public string Username { get; set; } = string.Empty;
    public string PasswordFile { get; set; } = string.Empty;
    public string KeyFile { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(PasswordFile) || string.IsNullOrWhiteSpace(KeyFile))
            return false;

        return true;
    }
    
    public List<EmployeeId> MandatoryOwnerIds { get; set; }
    public List<EmployeeId> StudentTeamOwnerIds { get; set; }
    public Dictionary<Grade, List<EmployeeId>> StudentTeamChannelOwnerIds { get; set; }

}
