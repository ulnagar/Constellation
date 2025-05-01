namespace Constellation.Application.Domains.GroupTutorials.Events.GroupTutorialCreated;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Core.DomainEvents;
using Core.Models.GroupTutorials;
using Helpers;
using Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTeam
    : IDomainEventHandler<GroupTutorialCreatedDomainEvent>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTeam(
        IGroupTutorialRepository groupTutorialRepository, 
        IMSTeamOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(GroupTutorialCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        // Create Team
        GroupTutorialCreatedMSTeamOperation operation = new()
        {
            DateScheduled = DateTime.Now,
            TeamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name),
            Action = MSTeamOperationAction.Add,
            TutorialId = notification.TutorialId
        };
        
        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}