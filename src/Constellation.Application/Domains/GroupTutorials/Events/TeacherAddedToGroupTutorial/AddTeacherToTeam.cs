﻿namespace Constellation.Application.Domains.GroupTutorials.Events.TeacherAddedToGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeacherToTeam
    : IDomainEventHandler<TeacherAddedToGroupTutorialDomainEvent>
{
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTeacherToTeam(
        IMSTeamOperationsRepository operationsRepository,
        IGroupTutorialRepository groupTutorialRepository,
        IUnitOfWork unitOfWork)
    {
        _operationsRepository = operationsRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TeacherAddedToGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        TutorialTeacher teacher = tutorial.Teachers.FirstOrDefault(teacher => teacher.Id == notification.TutorialTeacherId);

        if (teacher is null)
            return;

        // Create Team
        TeacherEmployedMSTeamOperation operation = new() 
        {
            DateScheduled = DateTime.Now,
            TeamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name),
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            StaffId = teacher.StaffId
        };

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
