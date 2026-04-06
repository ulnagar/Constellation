namespace Constellation.Application.Interfaces.Gateways.TeamsGateway.Models;

using System;
using System.Text.Json.Serialization;

public sealed class Team
{
    [JsonPropertyName("AllowAddRemoveApps")]
    public bool AllowAddRemoveApps { get; internal set; }

    [JsonPropertyName("AllowChannelMentions")]
    public bool AllowChannelMentions { get; internal set; }

    [JsonPropertyName("AllowCreatePrivateChannels")]
    public bool AllowCreatePrivateChannels { get; internal set; }

    [JsonPropertyName("AllowCreateUpdateChannels")]
    public bool AllowCreateUpdateChannels { get; internal set; }

    [JsonPropertyName("AllowCreateUpdateRemoveConnectors")]
    public bool AllowCreateUpdateRemoveConnectors { get; internal set; }

    [JsonPropertyName("AllowCreateUpdateRemoveTabs")]
    public bool AllowCreateUpdateRemoveTabs { get; internal set; }

    [JsonPropertyName("AllowCustomMemes")]
    public bool AllowCustomMemes { get; internal set; }

    [JsonPropertyName("AllowDeleteChannels")]
    public bool AllowDeleteChannels { get; internal set; }

    [JsonPropertyName("AllowGiphy")]
    public bool AllowGiphy { get; internal set; }

    [JsonPropertyName("AllowGuestCreateUpdateChannels")]
    public bool AllowGuestCreateUpdateChannels { get; internal set; }

    [JsonPropertyName("AllowGuestDeleteChannels")]
    public bool AllowGuestDeleteChannels { get; internal set; }

    [JsonPropertyName("AllowOwnerDeleteMessages")]
    public bool AllowOwnerDeleteMessages { get; internal set; }

    [JsonPropertyName("AllowStickersAndMemes")]
    public bool AllowStickersAndMemes { get; internal set; }

    [JsonPropertyName("AllowTeamMentions")]
    public bool AllowTeamMentions { get; internal set; }

    [JsonPropertyName("AllowUserDeleteMessages")]
    public bool AllowUserDeleteMessages { get; internal set; }

    [JsonPropertyName("AllowUserEditMessages")]
    public bool AllowUserEditMessages { get; internal set; }

    [JsonPropertyName("Archived")]
    public bool Archived { get; internal set; }

    [JsonPropertyName("Classification")]
    public string Classification { get; internal set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; internal set; } = string.Empty;

    [JsonPropertyName("DisplayName")]
    public string DisplayName { get; internal set; } = string.Empty;
    
    [JsonPropertyName("GroupId")]
    public Guid GroupId { get; internal set; }

    [JsonPropertyName("InternalId")]
    public string InternalId { get; internal set; } = string.Empty;

    [JsonPropertyName("MailNickName")]
    public string MailNickName { get; internal set; } = string.Empty;

    [JsonPropertyName("ShowInTeamsSearchAndSuggestions")]
    public bool ShowInTeamsSearchAndSuggestions { get; internal set; }

    [JsonPropertyName("Visibility")]
    public string Visibility { get; internal set; } = string.Empty;
}
