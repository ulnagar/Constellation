namespace Constellation.Application.Interfaces.Gateways.PowershellGateway.Models;

using System;

public sealed class TeamChannel
{
    public string Id { get; internal set; } = string.Empty;
    public string DisplayName { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public TeamChannelMembershipType MembershipType { get; internal set; }
    public Guid HostTeamId { get; internal set; }
    public Guid TenantId { get; internal set; }
}
