namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Events.GroupTutorialCreated;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Core.DomainEvents;
using Core.Models.GroupTutorials;
using Core.Models.Operations;
using Core.Models.Operations.Repositories;
using Helpers;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTeam
    : IDomainEventHandler<GroupTutorialCreatedDomainEvent>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTeam(
        IGroupTutorialRepository groupTutorialRepository, 
        ITeamOperationRepository operationsRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(GroupTutorialCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        GroupTutorial? tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        // Create Team
        CreateTeamTeamOperation operation = new(
            MicrosoftTeamsHelper.FormatTeamName(tutorial.Name),
            "8912;GTUT;Support;");

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}