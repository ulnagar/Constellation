namespace Constellation.Application.GroupTutorials.CreateRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateRollCommandHandler : ICommandHandler<CreateRollCommand, TutorialRollId>
{
    private readonly ITutorialRollRepository _rollRepository;
	private readonly IGroupTutorialRepository _tutorialRepository;
	private readonly IUnitOfWork _unitOfWork;

    public CreateRollCommandHandler(IUnitOfWork unitOfWork,
        IGroupTutorialRepository tutorialRepository,
        ITutorialRollRepository rollRepository)
    {
        _unitOfWork = unitOfWork;
        _tutorialRepository = tutorialRepository;
        _rollRepository = rollRepository;
    }

    public async Task<Result<TutorialRollId>> Handle(CreateRollCommand request, CancellationToken cancellationToken)
    {
        var tutorial = await _tutorialRepository.GetWholeAggregate(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure<TutorialRollId>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId.Value));

        Result<TutorialRoll> rollResult = tutorial.CreateRoll(request.RollDate);

        if (rollResult.IsFailure)
        {
            //TODO: Log error
            return Result.Failure<TutorialRollId>(rollResult.Error);
        }

        _rollRepository.Insert(rollResult.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return rollResult.Value.Id;
    }
}
