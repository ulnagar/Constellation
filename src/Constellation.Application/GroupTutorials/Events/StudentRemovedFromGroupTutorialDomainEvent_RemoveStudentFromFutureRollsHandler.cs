namespace Constellation.Application.GroupTutorials.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class StudentRemovedFromGroupTutorialDomainEvent_RemoveStudentFromFutureRollsHandler
    : IDomainEventHandler<StudentRemovedFromGroupTutorialDomainEvent>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ITutorialEnrolmentRepository _tutorialEnrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StudentRemovedFromGroupTutorialDomainEvent_RemoveStudentFromFutureRollsHandler(
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
        var tutorial = await _groupTutorialRepository.GetWithRollsById(notification.TutorialId, cancellationToken);

        if (tutorial is null)
            return;

        var enrolment = await _tutorialEnrolmentRepository.GetById(notification.EnrolmentId, cancellationToken);

        if (enrolment is null)
            return;

        // Find all future non-marked rolls to remove student from
        var rolls = tutorial.Rolls
            .Where(roll => 
                roll.SessionDate > DateOnly.FromDateTime(DateTime.Today) && 
                roll.Status == TutorialRollStatus.Unsubmitted)
            .ToList();

        foreach (var roll in rolls)
        {
            roll.RemoveStudent(enrolment.StudentId);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
