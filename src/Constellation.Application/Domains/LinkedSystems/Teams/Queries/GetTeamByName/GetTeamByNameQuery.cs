namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetTeamByName;

using Abstractions.Messaging;
using Models;

public sealed record GetTeamByNameQuery(
    string Name)
    : IQuery<TeamResource>;
