namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetTeamByName;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models;
using Core.Shared;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTeamByNameQueryHandler
    : IQueryHandler<GetTeamByNameQuery, TeamResource>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamByNameQueryHandler(
        ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Result<TeamResource>> Handle(GetTeamByNameQuery request, CancellationToken cancellationToken)
    {
        List<Team> teams = await _teamRepository.GetByName(request.Name, cancellationToken);

        if (teams.Count == 0)
            return Result.Failure<TeamResource>(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);

        Team exactMatch = teams.FirstOrDefault(team => team.Name == request.Name);

        if (exactMatch is not null)
        {
            return new TeamResource(
                exactMatch.Id,
                exactMatch.Name,
                exactMatch.Description,
                exactMatch.Link,
                exactMatch.IsArchived);
        }

        return Result.Failure<TeamResource>(DomainErrors.LinkedSystems.Teams.MoreThanOneMatchFound);
    }
}
