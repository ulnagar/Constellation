﻿namespace Constellation.Application.GroupTutorials.Events;

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

internal sealed class StudentRemovedFromGroupTutorialDomainEvent_RemoveStudentFromTeamHandler
    : IDomainEventHandler<StudentRemovedFromGroupTutorialDomainEvent>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ITutorialEnrolmentRepository _tutorialEnrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StudentRemovedFromGroupTutorialDomainEvent_RemoveStudentFromTeamHandler(
        IGroupTutorialRepository groupTutorialRepository,
        ITutorialEnrolmentRepository tutorialEnrolmentRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _tutorialEnrolmentRepository = tutorialEnrolmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StudentRemovedFromGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        var enrolment = await _tutorialEnrolmentRepository.GetById(notification.EnrolmentId, cancellationToken);

        if (enrolment is null)
            return;

        // Create Team
        var operation = new StudentEnrolledMSTeamOperation
        {
            DateScheduled = DateTime.Now,
            TeamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name),
            Action = MSTeamOperationAction.Remove,
            StudentId = enrolment.StudentId
        };

        _unitOfWork.Add(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}