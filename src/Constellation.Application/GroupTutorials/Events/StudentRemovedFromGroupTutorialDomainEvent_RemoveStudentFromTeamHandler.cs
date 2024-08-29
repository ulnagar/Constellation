namespace Constellation.Application.GroupTutorials.Events;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Core.DomainEvents;
using Helpers;
using Interfaces.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class StudentRemovedFromGroupTutorialDomainEvent_RemoveStudentFromTeamHandler
    : IDomainEventHandler<StudentRemovedFromGroupTutorialDomainEvent>
{
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StudentRemovedFromGroupTutorialDomainEvent_RemoveStudentFromTeamHandler(
        IMSTeamOperationsRepository operationsRepository,
        IGroupTutorialRepository groupTutorialRepository,
        IUnitOfWork unitOfWork)
    {
        _operationsRepository = operationsRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StudentRemovedFromGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        TutorialEnrolment enrolment = tutorial.Enrolments.FirstOrDefault(enrolment => enrolment.Id == notification.EnrolmentId);

        if (enrolment is null)
            return;

        // Create Team
        StudentEnrolledMSTeamOperation operation = new()
        {
            DateScheduled = DateTime.Now,
            TeamName = MicrosoftTeamsHelper.FormatTeamName(tutorial.Name),
            Action = MSTeamOperationAction.Remove,
            StudentId = enrolment.StudentId
        };

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
