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

internal sealed class TeacherAddedToGroupTutorialDomainEvent_AddTeacherToTeamHandler
    : IDomainEventHandler<TeacherAddedToGroupTutorialDomainEvent>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ITutorialTeacherRepository _tutorialTeacherRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TeacherAddedToGroupTutorialDomainEvent_AddTeacherToTeamHandler(
        IGroupTutorialRepository groupTutorialRepository,
        ITutorialTeacherRepository tutorialTeacherRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _tutorialTeacherRepository = tutorialTeacherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TeacherAddedToGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        var teacher = await _tutorialTeacherRepository.GetById(notification.TutorialTeacherId, cancellationToken);

        if (teacher is null)
            return;

        // Create Team
        var operation = new TeacherEmployedMSTeamOperation
        {
            DateScheduled = DateTime.Now,
            TeamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name),
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            StaffId = teacher.StaffId
        };

        _unitOfWork.Add(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
