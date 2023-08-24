namespace Constellation.Application.GroupTutorials.Events;

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

internal sealed class TeacherRemovedFromGroupTutorialDomainEvent_RemoveTeacherFromTeamHandler
    : IDomainEventHandler<TeacherRemovedFromGroupTutorialDomainEvent>
{
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TeacherRemovedFromGroupTutorialDomainEvent_RemoveTeacherFromTeamHandler(
        IMSTeamOperationsRepository operationsRepository,
        IGroupTutorialRepository groupTutorialRepository,
        IUnitOfWork unitOfWork)
    {
        _operationsRepository = operationsRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TeacherRemovedFromGroupTutorialDomainEvent notification, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(notification.GroupTutorialId, cancellationToken);

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
            Action = MSTeamOperationAction.Remove,
            StaffId = teacher.StaffId
        };

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
