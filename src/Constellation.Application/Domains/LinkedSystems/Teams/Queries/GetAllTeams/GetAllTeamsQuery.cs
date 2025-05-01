namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetAllTeams;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllTeamsQuery : IQuery<List<TeamResource>>;
