namespace Constellation.Application.SciencePracs.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SciencePracLessonCreatedDomainEvent_CreateRolls
    : IDomainEventHandler<SciencePracLessonCreatedDomainEvent>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SciencePracLessonCreatedDomainEvent_CreateRolls(
        ILessonRepository lessonRepository,
        ICourseOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<SciencePracLessonCreatedDomainEvent_CreateRolls>();
    }
    public async Task Handle(SciencePracLessonCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(notification.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", notification.LessonId);

            return;
        }

        List<string> studentIds = new();

        foreach (SciencePracLessonOffering record in lesson.Offerings)
        {
            CourseOffering offering = await _offeringRepository.GetById(record.OfferingId, cancellationToken);

            if (offering is null)
                continue;

            studentIds.AddRange(offering.Enrolments.Select(enrolment => enrolment.StudentId).ToList());
        }

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        var groupedStudents = students.GroupBy(student => student.SchoolCode);

        foreach (var schoolGroup in groupedStudents)
        {
            SciencePracRoll roll = new(
                lesson.Id,
                schoolGroup.Key);

            foreach (Student student in schoolGroup)
                roll.AddStudent(student.StudentId);

            lesson.AddRoll(roll);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
