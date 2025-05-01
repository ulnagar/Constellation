namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetCurrentTeamsWithMembership;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentTeamsWithMembershipQuery()
    : IQuery<List<TeamWithMembership>>;
