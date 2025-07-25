namespace Constellation.Application.Domains.Tutorials.Commands.AddTeamToTutorial;

using Abstractions.Messaging;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeamToTutorialCommandHandler
: ICommandHandler<AddTeamToTutorialCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTeamToTutorialCommandHandler(
        ITutorialRepository tutorialRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddTeamToTutorialCommand>();
    }

    public async Task<Result> Handle(AddTeamToTutorialCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(AddTeamToTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Failed to add Team to tutorial");

            return Result.Failure(TutorialErrors.NotFound(request.TutorialId));
        }

        Team team = await _teamRepository.GetById(request.TeamId, cancellationToken);

        if (team is null)
        {
            _logger
                .ForContext(nameof(AddTeamToTutorialCommand), request, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase, true)
                .Warning("Failed to add Team to tutorial");

            return Result.Failure(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

        Result result = tutorial.AddTeam(
            team.Id,
            team.Name,
            team.Link);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddTeamToTutorialCommand), request, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add Team to tutorial");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
