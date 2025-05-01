namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetTeamById;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Shared;
using Models;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTeamByIdQueryHandler
    : IQueryHandler<GetTeamByIdQuery, TeamResource>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamByIdQueryHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Result<TeamResource>> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetById(request.Id, cancellationToken);

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
