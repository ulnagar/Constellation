namespace Constellation.Application.Teams.GetCurrentTeamsWithMembership;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentTeamsWithMembershipQuery()
    : IQuery<List<TeamWithMembership>>;
