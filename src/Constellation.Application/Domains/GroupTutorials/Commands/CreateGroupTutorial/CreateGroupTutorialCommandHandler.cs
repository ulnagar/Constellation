namespace Constellation.Application.Domains.GroupTutorials.Commands.CreateGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateGroupTutorialCommandHandler : ICommandHandler<CreateGroupTutorialCommand, GroupTutorialId>
{
    private readonly IGroupTutorialRepository _tutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGroupTutorialCommandHandler(IUnitOfWork unitOfWork,
        IGroupTutorialRepository tutorialRepository)
    {
        _unitOfWork = unitOfWork;
        _tutorialRepository = tutorialRepository;
    }

    public async Task<Result<GroupTutorialId>> Handle(CreateGroupTutorialCommand request, CancellationToken cancellationToken)
    {
        Result<GroupTutorial> tutorialResult = GroupTutorial.Create(
            new GroupTutorialId(Guid.NewGuid()),
            request.Name,
            request.StartDate,
            request.EndDate);

        if (tutorialResult.IsFailure)
        {
            //TODO: Log error
            return Result.Failure<GroupTutorialId>(DomainErrors.GroupTutorials.GroupTutorial.CouldNotCreateTutorial);
        }

        _tutorialRepository.Insert(tutorialResult.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return tutorialResult.Value.Id;
    }
}
