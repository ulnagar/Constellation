namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetCurrentTeamsWithMembership;

using System;
using System.Collections.Generic;

public sealed record TeamWithMembership(
    Guid Id,
    string Name,
    string Description,
    List<TeamWithMembership.Member> Members)
{
    public sealed record Member(
        string EmailAddress,
        string PermissionLevel,
        Dictionary<string, string> Channels);
}
