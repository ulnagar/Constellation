namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetTeamById;

using Abstractions.Messaging;
using Models;
using System;

public sealed record GetTeamByIdQuery(
    Guid Id)
    : IQuery<TeamResource>;
