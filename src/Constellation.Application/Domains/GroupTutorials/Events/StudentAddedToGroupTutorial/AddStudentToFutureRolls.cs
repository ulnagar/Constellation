namespace Constellation.Application.Domains.GroupTutorials.Events.StudentAddedToGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Enums;
using Constellation.Core.Models.GroupTutorials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStudentToFutureRolls
    : IDomainEventHandler<StudentAddedToGroupTutorialDomainEvent>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStudentToFutureRolls(
        IGroupTutorialRepository groupTutorialRepository,
        IUnitOfWork unitOfWork)
    {
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

        // Find all future non-marked rolls to add student to
        List<TutorialRoll> rolls = tutorial.Rolls
            .Where(roll => 
                roll.SessionDate > DateOnly.FromDateTime(DateTime.Today) && 
                roll.Status == TutorialRollStatus.Unsubmitted)
            .ToList();

        foreach (TutorialRoll roll in rolls)
            roll.AddStudent(enrolment.StudentId, true);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
