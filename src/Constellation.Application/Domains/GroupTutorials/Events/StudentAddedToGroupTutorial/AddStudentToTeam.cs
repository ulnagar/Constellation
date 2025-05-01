namespace Constellation.Application.Domains.GroupTutorials.Events.StudentAddedToGroupTutorial;

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

internal sealed class AddStudentToTeam
    : IDomainEventHandler<StudentAddedToGroupTutorialDomainEvent>
{
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStudentToTeam(
        IMSTeamOperationsRepository operationsRepository,
        IGroupTutorialRepository groupTutorialRepository,
        IUnitOfWork unitOfWork)
    {
        _operationsRepository = operationsRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StudentAddedToGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
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
            Action = MSTeamOperationAction.Add,
            StudentId = enrolment.StudentId
        };

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
