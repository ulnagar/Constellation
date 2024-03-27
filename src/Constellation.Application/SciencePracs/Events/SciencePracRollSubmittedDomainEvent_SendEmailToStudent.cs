namespace Constellation.Application.SciencePracs.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SciencePracRollSubmittedDomainEvent_SendEmailToStudent
    : IDomainEventHandler<SciencePracRollSubmittedDomainEvent>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SciencePracRollSubmittedDomainEvent_SendEmailToStudent(
        ILessonRepository lessonRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _emailService = emailService;
        _logger = logger.ForContext<SciencePracRollSubmittedDomainEvent>();
    }

    public async Task Handle(SciencePracRollSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(notification.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", notification.LessonId);

            return;
        }

        SciencePracRoll roll = lesson.Rolls.FirstOrDefault(roll => roll.Id == notification.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find Science Prac Roll with Id {id}", notification.RollId);

            return;
        }

        Course course = await _courseRepository.GetByLessonId(lesson.Id, cancellationToken);

        string courseName = course?.Name ?? string.Empty;

        List<SciencePracAttendance> presentStudents = roll.Attendance.Where(record => record.Present).ToList();

        foreach (SciencePracAttendance attendance in presentStudents)
        {
            Student student = await _studentRepository.GetById(attendance.StudentId, cancellationToken);

            await _emailService.SendStudentLessonCompletedEmail(student, lesson.Name, courseName, cancellationToken);
        }
    }
}
