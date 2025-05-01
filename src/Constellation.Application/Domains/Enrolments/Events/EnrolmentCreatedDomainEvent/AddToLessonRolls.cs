namespace Constellation.Application.Domains.Enrolments.Events.EnrolmentCreatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Enums;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddToLessonRolls
    : IDomainEventHandler<EnrolmentCreatedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddToLessonRolls(
        IStudentRepository studentRepository,
        ILessonRepository lessonRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _lessonRepository = lessonRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<EnrolmentCreatedDomainEvent>();
    }

    public async Task Handle(EnrolmentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        SchoolEnrolment enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolEnrolmentErrors.NotFound, true)
                .Error("Failed to complete the event handler");

            return;
        }

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForOffering(notification.OfferingId);

        lessons = lessons
            .Where(lesson =>
                lesson.DueDate >= _dateTime.Today)
            .ToList();

        List<SciencePracRoll> rolls = lessons
            .SelectMany(lesson =>
                lesson.Rolls.Where(roll =>
                    roll.SchoolCode == enrolment.SchoolCode))
            .ToList();

        foreach (SciencePracRoll roll in rolls)
        {
            if (roll.Status == LessonStatus.Cancelled)
                roll.ReinstateRoll();

            if (roll.Attendance.Any(attendance => attendance.StudentId == student.Id))
                continue;

            roll.AddStudent(student.Id);
        }

        List<SciencePracLesson> newRollsRequired = lessons
            .Where(lesson =>
                lesson.Rolls.All(roll =>
                    roll.SchoolCode != enrolment.SchoolCode))
            .ToList();

        foreach (SciencePracLesson lesson in newRollsRequired)
        {
            SciencePracRoll roll = new(
                lesson.Id,
                enrolment.SchoolCode);

            roll.AddStudent(student.Id);

            lesson.AddRoll(roll);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
