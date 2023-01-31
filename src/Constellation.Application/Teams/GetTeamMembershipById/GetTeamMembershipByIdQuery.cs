namespace Constellation.Application.Teams.GetTeamMembershipById;

using Constellation.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;

public sealed record GetTeamMembershipByIdQuery(
    Guid Id) 
    : IQuery<List<TeamMembershipResponse>>;