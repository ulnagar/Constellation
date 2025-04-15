namespace Constellation.Application.Interfaces.Gateways.PowershellGateway.Models;

using Newtonsoft.Json;
using System;

public sealed class TeamChannel
{
    [JsonProperty("Id")]
    public string Id { get; internal set; } = string.Empty;

    [JsonProperty("DisplayName")]
    public string DisplayName { get; internal set; } = string.Empty;

    [JsonProperty("Description")]
    public string Description { get; internal set; } = string.Empty;

    [JsonProperty("MembershipType")]
    public TeamChannelMembershipType MembershipType { get; internal set; }

    [JsonProperty("HostTeamId")]
    public Guid HostTeamId { get; internal set; }

    [JsonProperty("TenantId")]
    public Guid TenantId { get; internal set; }
}
