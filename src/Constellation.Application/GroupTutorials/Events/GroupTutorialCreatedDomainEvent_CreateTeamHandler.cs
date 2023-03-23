namespace Constellation.Application.GroupTutorials.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GroupTutorialCreatedDomainEvent_CreateTeamHandler
    : IDomainEventHandler<GroupTutorialCreatedDomainEvent>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GroupTutorialCreatedDomainEvent_CreateTeamHandler(IGroupTutorialRepository groupTutorialRepository, IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task Handle(GroupTutorialCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        // Create Team
        var operation = new GroupTutorialCreatedMSTeamOperation
        {
            DateScheduled = DateTime.Now,
            TeamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name),
            Action = MSTeamOperationAction.Add,
            TutorialId = notification.TutorialId
        };

        _unitOfWork.Add(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}