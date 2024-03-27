namespace Constellation.Application.Teams.GetAllTeams;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Teams.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
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
        var teams = await _teamRepository.GetAll(cancellationToken);

        var dataList = new List<TeamResource>();

        if (teams is null)
        {
            return dataList;
        }

        foreach (var team in teams)
        {
            var entry = new TeamResource(
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
