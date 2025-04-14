namespace Constellation.Application.Interfaces.Gateways.PowershellGateway.Models;

using System;

public sealed class Team
{
    public bool AllowAddRemoveApps { get; internal set; }
    public bool AllowChannelMentions { get; internal set; }
    public bool AllowCreatePrivateChannels { get; internal set; }
    public bool AllowCreateUpdateChannels { get; internal set; }
    public bool AllowCreateUpdateRemoveConnectors { get; internal set; }
    public bool AllowCreateUpdateRemoveTabs { get; internal set; }
    public bool AllowCustomMemes { get; internal set; }
    public bool AllowDeleteChannels { get; internal set; }
    public bool AllowGiphy { get; internal set; }
    public bool AllowGuestCreateUpdateChannels { get; internal set; }
    public bool AllowGuestDeleteChannels { get; internal set; }
    public bool AllowOwnerDeleteMessages { get; internal set; }
    public bool AllowStickersAndMemes { get; internal set; }
    public bool AllowTeamMentions { get; internal set; }
    public bool AllowUserDeleteMessages { get; internal set; }
    public bool AllowUserEditMessages { get; internal set; }
    public bool Archived { get; internal set; }
    public string Classification { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public string DisplayName { get; internal set; } = string.Empty;
    public Guid GroupId { get; internal set; }
    public string InternalId { get; internal set; } = string.Empty;
    public string MailNickName { get; internal set; } = string.Empty;
    public bool ShowInTeamsSearchAndSuggestions { get; internal set; }
    public string Visibility { get; internal set; } = string.Empty;
}
