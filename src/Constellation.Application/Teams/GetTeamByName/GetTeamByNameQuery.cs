namespace Constellation.Application.Teams.GetTeamByName;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Teams.Models;
using System;

public sealed record GetTeamByNameQuery(
    string Name)
    : IQuery<TeamResource>;
