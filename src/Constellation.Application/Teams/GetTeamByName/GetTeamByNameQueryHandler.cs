namespace Constellation.Application.Teams.GetTeamByName;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Teams.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
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
        var team = await _teamRepository.GetByName(request.Name, cancellationToken);

        if (team is null)
        {
            return Result.Failure<TeamResource>(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

        return new TeamResource(
            team.Id,
            team.Name,
            team.Description,
            team.Link,
            team.IsArchived);
    }
}
