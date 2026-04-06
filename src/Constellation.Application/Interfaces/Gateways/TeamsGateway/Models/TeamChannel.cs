namespace Constellation.Application.Interfaces.Gateways.TeamsGateway.Models;

using System;
using System.Text.Json.Serialization;

public sealed class TeamChannel
{
    [JsonPropertyName("Id")]
    public string Id { get; internal set; } = string.Empty;

    [JsonPropertyName("DisplayName")]
    public string DisplayName { get; internal set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; internal set; } = string.Empty;

    [JsonPropertyName("MembershipType")]
    public TeamChannelMembershipType MembershipType { get; internal set; }

    [JsonPropertyName("HostTeamId")]
    public Guid HostTeamId { get; internal set; }

    [JsonPropertyName("TenantId")]
    public Guid TenantId { get; internal set; }
}
