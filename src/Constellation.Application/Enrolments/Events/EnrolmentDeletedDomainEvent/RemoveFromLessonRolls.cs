namespace Constellation.Application.Enrolments.Events.EnrolmentDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.SciencePracs;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings.Identifiers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFromLessonRolls
    : IDomainEventHandler<EnrolmentDeletedDomainEvent>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveFromLessonRolls(
        ILessonRepository lessonRepository,
        IEnrolmentRepository enrolmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _enrolmentRepository = enrolmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<EnrolmentDeletedDomainEvent>();
    }

    public async Task Handle(EnrolmentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForStudent(notification.StudentId, cancellationToken);

        if (!lessons.Any())
            return;

        lessons = lessons
            .Where(lesson =>
                lesson.Offerings.Any(record =>
                    record.OfferingId == notification.OfferingId))
            .ToList();

        if (!lessons.Any())
            return;

        // Ensure student isn't also enrolled in another current class that is covered by these lessons
        // (e.g. student is marked withdrawn from 07SCIP1 but enrolled in 07SCIP2)
        List<Enrolment> currentEnrolments = await _enrolmentRepository.GetCurrentByStudentId(notification.StudentId, cancellationToken);
        List<OfferingId> currentEnrolmentOfferingIds = currentEnrolments
            .Select(enrolment => enrolment.OfferingId)
            .ToList();

        lessons = lessons
            .Where(lesson =>
                lesson.Offerings.All(offering => !currentEnrolmentOfferingIds.Contains(offering.OfferingId)))
            .ToList();
        
        List<SciencePracRoll> rolls = lessons
            .SelectMany(lesson =>
                lesson.Rolls.Where(roll =>
                    roll.Attendance.Any(attendance =>
                        attendance.StudentId == notification.StudentId) &&
                    roll.Status == LessonStatus.Active))
            .ToList();

        foreach (SciencePracRoll roll in rolls)
        {
            if (roll.Attendance.Count == 1)
            {
                // This is the last student in the class from this school
                // The roll is no longer needed and should be cancelled
                roll.CancelRoll("Last student has withdrawn. Roll no longer required.");
            }

            // Remove the student from the roll
            SciencePracAttendance? attendance = roll.RemoveStudent(notification.StudentId);

            if (attendance is not null)
            {
                _lessonRepository.Delete(attendance);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}