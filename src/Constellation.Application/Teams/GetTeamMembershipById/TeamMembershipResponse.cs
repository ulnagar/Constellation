namespace Constellation.Application.Teams.GetTeamMembershipById;

using System;
using System.Collections.Generic;

public sealed record TeamMembershipResponse(
    Guid TeamId,
    string EmailAddress,
    string PermissionLevel,
    List<TeamMembershipResponse.TeamMembershipChannelResponse> Channels = null)
{
    public sealed record TeamMembershipChannelResponse(
        string ChannelName,
        string PermissionLevel);
}