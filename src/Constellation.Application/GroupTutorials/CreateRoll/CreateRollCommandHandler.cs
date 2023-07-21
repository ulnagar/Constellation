namespace Constellation.Application.GroupTutorials.CreateRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateRollCommandHandler : ICommandHandler<CreateRollCommand, TutorialRollId>
{
	private readonly IGroupTutorialRepository _tutorialRepository;
	private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateRollCommandHandler(
        IGroupTutorialRepository tutorialRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateRollCommand>();
    }

    public async Task<Result<TutorialRollId>> Handle(CreateRollCommand request, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure<TutorialRollId>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        Result<TutorialRoll> rollResult = tutorial.CreateRoll(request.RollDate);

        if (rollResult.IsFailure)
        {
            _logger
                .ForContext("Error", rollResult.Error, true)
                .Warning("Could not create roll due to error");

            return Result.Failure<TutorialRollId>(rollResult.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return rollResult.Value.Id;
    }
}
