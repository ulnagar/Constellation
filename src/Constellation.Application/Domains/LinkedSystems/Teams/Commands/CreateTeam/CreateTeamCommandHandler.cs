namespace Constellation.Application.Domains.LinkedSystems.Teams.Commands.CreateTeam;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

public sealed class CreateTeamCommandHandler 
    : ICommandHandler<CreateTeamCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamRepository _teamRepository;

    public CreateTeamCommandHandler(IUnitOfWork unitOfWork, ITeamRepository teamRepository)
    {
        _unitOfWork = unitOfWork;
        _teamRepository = teamRepository;
    }

    public async Task<Result> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var checkTeam = await _teamRepository.GetById(request.Id, cancellationToken);

        if (checkTeam != null)
        {
            return Result.Failure(DomainErrors.LinkedSystems.Teams.AlreadyExists(request.Id));
        }

        var team = Team.Create(
            request.Id,
            request.Name,
            request.Description,
            request.ChannelId
        );

        _teamRepository.Insert(team);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
