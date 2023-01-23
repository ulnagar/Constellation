namespace Constellation.Application.Teams.GetAllTeams;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Teams.Models;
using System.Collections.Generic;

public sealed record GetAllTeamsQuery : IQuery<List<TeamResource>>;
