namespace Constellation.Application.Teams.DeleteTeam;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteTeamCommandHandler
    : ICommandHandler<DeleteTeamCommand>
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTeamCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetById(request.Id, cancellationToken);

        if (team is null)
        {
            return Result.Failure(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

        _teamRepository.Remove(team);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
