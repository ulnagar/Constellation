namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetTeamMembershipById;

using Abstractions.Messaging;
using System;
using System.Collections.Generic;

public sealed record GetTeamMembershipByIdQuery(
    Guid Id) 
    : IQuery<List<TeamMembershipResponse>>;