namespace Constellation.Application.Interfaces.Gateways.TeamsGateway.Models;

using Newtonsoft.Json;
using System;

public sealed class Team
{
    [JsonProperty("AllowAddRemoveApps")]
    public bool AllowAddRemoveApps { get; internal set; }

    [JsonProperty("AllowChannelMentions")]
    public bool AllowChannelMentions { get; internal set; }

    [JsonProperty("AllowCreatePrivateChannels")]
    public bool AllowCreatePrivateChannels { get; internal set; }

    [JsonProperty("AllowCreateUpdateChannels")]
    public bool AllowCreateUpdateChannels { get; internal set; }

    [JsonProperty("AllowCreateUpdateRemoveConnectors")]
    public bool AllowCreateUpdateRemoveConnectors { get; internal set; }

    [JsonProperty("AllowCreateUpdateRemoveTabs")]
    public bool AllowCreateUpdateRemoveTabs { get; internal set; }

    [JsonProperty("AllowCustomMemes")]
    public bool AllowCustomMemes { get; internal set; }

    [JsonProperty("AllowDeleteChannels")]
    public bool AllowDeleteChannels { get; internal set; }

    [JsonProperty("AllowGiphy")]
    public bool AllowGiphy { get; internal set; }

    [JsonProperty("AllowGuestCreateUpdateChannels")]
    public bool AllowGuestCreateUpdateChannels { get; internal set; }

    [JsonProperty("AllowGuestDeleteChannels")]
    public bool AllowGuestDeleteChannels { get; internal set; }

    [JsonProperty("AllowOwnerDeleteMessages")]
    public bool AllowOwnerDeleteMessages { get; internal set; }

    [JsonProperty("AllowStickersAndMemes")]
    public bool AllowStickersAndMemes { get; internal set; }

    [JsonProperty("AllowTeamMentions")]
    public bool AllowTeamMentions { get; internal set; }

    [JsonProperty("AllowUserDeleteMessages")]
    public bool AllowUserDeleteMessages { get; internal set; }

    [JsonProperty("AllowUserEditMessages")]
    public bool AllowUserEditMessages { get; internal set; }

    [JsonProperty("Archived")]
    public bool Archived { get; internal set; }

    [JsonProperty("Classification")]
    public string Classification { get; internal set; } = string.Empty;

    [JsonProperty("Description")]
    public string Description { get; internal set; } = string.Empty;

    [JsonProperty("DisplayName")]
    public string DisplayName { get; internal set; } = string.Empty;
    
    [JsonProperty("GroupId")]
    public Guid GroupId { get; internal set; }

    [JsonProperty("InternalId")]
    public string InternalId { get; internal set; } = string.Empty;

    [JsonProperty("MailNickName")]
    public string MailNickName { get; internal set; } = string.Empty;

    [JsonProperty("ShowInTeamsSearchAndSuggestions")]
    public bool ShowInTeamsSearchAndSuggestions { get; internal set; }

    [JsonProperty("Visbility")]
    public string Visibility { get; internal set; } = string.Empty;
}
