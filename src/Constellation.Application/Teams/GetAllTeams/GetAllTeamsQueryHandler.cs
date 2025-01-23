﻿namespace Constellation.Application.Teams.GetAllTeams;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Teams.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllTeamsQueryHandler
    : IQueryHandler<GetAllTeamsQuery, List<TeamResource>>
{
    private readonly ITeamRepository _teamRepository;

    public GetAllTeamsQueryHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Result<List<TeamResource>>> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
    {
        List<Team> teams = await _teamRepository.GetAll(cancellationToken);

        List<TeamResource> dataList = new();

        if (teams.Count == 0)
            return dataList;

        foreach (Team team in teams.OrderBy(entry => entry.Name))
        {
            TeamResource entry = new(
                team.Id,
                team.Name,
                team.Description,
                team.Link,
                team.IsArchived);

            dataList.Add(entry);
        }

        return dataList;
    }
}
