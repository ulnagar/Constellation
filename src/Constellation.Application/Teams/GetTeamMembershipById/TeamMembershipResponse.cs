namespace Constellation.Application.Teams.GetTeamMembershipById;

using System;

public sealed record TeamMembershipResponse(
    Guid TeamId,
    string EmailAddress,
    string PermissionLevel);