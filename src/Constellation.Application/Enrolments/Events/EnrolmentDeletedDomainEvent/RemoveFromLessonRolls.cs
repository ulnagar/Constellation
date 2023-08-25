namespace Constellation.Application.Enrolments.Events.EnrolmentDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Enrolment.Events;
using Constellation.Core.Models.SciencePracs;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFromLessonRolls
    : IDomainEventHandler<EnrolmentDeletedDomainEvent>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveFromLessonRolls(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<EnrolmentDeletedDomainEvent>();
    }

    public async Task Handle(EnrolmentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForStudent(notification.StudentId);

        if (!lessons.Any())
            return;

        lessons = lessons
            .Where(lesson =>
                lesson.Offerings.Any(record =>
                    record.OfferingId == notification.OfferingId))
            .ToList();

        if (!lessons.Any())
            return;

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
                SciencePracLesson lesson = lessons.First(lesson => lesson.Id == roll.LessonId);

                lesson.Cancel();
            }

            roll.RemoveStudent(notification.StudentId);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}