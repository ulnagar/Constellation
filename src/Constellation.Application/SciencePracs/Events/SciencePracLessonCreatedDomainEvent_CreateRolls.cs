﻿namespace Constellation.Application.SciencePracs.Events;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.DomainEvents;
using Core.Models.OfferingEnrolments;
using Core.Models.OfferingEnrolments.Repositories;
using Core.Models.Students.Identifiers;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SciencePracLessonCreatedDomainEvent_CreateRolls
    : IDomainEventHandler<SciencePracLessonCreatedDomainEvent>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SciencePracLessonCreatedDomainEvent_CreateRolls(
        ILessonRepository lessonRepository,
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
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

        List<StudentId> studentIds = new();

        foreach (SciencePracLessonOffering record in lesson.Offerings)
        {
            List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByOfferingId(record.OfferingId, cancellationToken);

            foreach (OfferingEnrolment enrolment in enrolments)
                studentIds.Add(enrolment.StudentId);
        }

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        IEnumerable<IGrouping<string, Student>> groupedStudents = students.GroupBy(student => student.CurrentEnrolment?.SchoolCode);

        foreach (IGrouping<string, Student> schoolGroup in groupedStudents)
        {
            SciencePracRoll roll = new(
                lesson.Id,
                schoolGroup.Key);

            foreach (Student student in schoolGroup)
                roll.AddStudent(student.Id);

            lesson.AddRoll(roll);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
