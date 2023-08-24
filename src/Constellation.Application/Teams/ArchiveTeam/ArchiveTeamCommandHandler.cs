namespace Constellation.Application.Teams.ArchiveTeam;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ArchiveTeamCommandHandler
    : ICommandHandler<ArchiveTeamCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamRepository _teamRepository;

    public ArchiveTeamCommandHandler(IUnitOfWork unitOfWork, ITeamRepository teamRepository)
    {
        _unitOfWork = unitOfWork;
        _teamRepository = teamRepository;
    }
    public async Task<Result> Handle(ArchiveTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetById(request.Id, cancellationToken);

        if (team is null)
        {
            return Result.Failure(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

        if (team.IsArchived)
        {
            return Result.Success();
        }

        team.ArchiveTeam();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
