namespace Constellation.Application.Teams.GetTeamMembershipById;

using Constellation.Core.Enums;
using System;

public sealed record TeamMembershipResponse(
    Guid TeamId,
    string EmailAddress,
    string PermissionLevel);