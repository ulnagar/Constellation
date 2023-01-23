namespace Constellation.Application.Teams.GetTeamById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Teams.Models;
using System;

public sealed record GetTeamByIdQuery(
    Guid Id)
    : IQuery<TeamResource>;
