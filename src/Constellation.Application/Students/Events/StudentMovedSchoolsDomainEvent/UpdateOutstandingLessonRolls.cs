namespace Constellation.Application.Students.Events.StudentMovedSchoolsDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students.Errors;
using Core.Models.Students.Events;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateOutstandingLessonRolls
: IDomainEventHandler<StudentMovedSchoolsDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateOutstandingLessonRolls(
        IStudentRepository studentRepository,
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<StudentMovedSchoolsDomainEvent>();
    }

    public async Task Handle(StudentMovedSchoolsDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Updating lesson rolls for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.PreviousSchoolCode, notification.CurrentSchoolCode);

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentMovedSchoolsDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to process Student school change");

            return;
        }

        List<OfferingId> offeringIds = student.Enrolments
            .Where(enrolment => !enrolment.IsDeleted)
            .Select(enrolments => enrolments.OfferingId)
            .Distinct()
            .ToList();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForStudent(notification.StudentId, cancellationToken);

        List<SciencePracRoll> oldRolls = lessons
            .SelectMany(lesson => lesson.Rolls)
            .Where(roll =>
                roll.SchoolCode == notification.PreviousSchoolCode &&
                roll.Status == LessonStatus.Active &&
                roll.Attendance.Any(attendance => attendance.StudentId == notification.StudentId))
            .ToList();

        List<SciencePracLessonId> updatedLessonIds = new();

        foreach (SciencePracRoll roll in oldRolls)
        {
            SciencePracLesson lesson = lessons.First(lesson => lesson.Id == roll.LessonId);

            if (roll.Attendance.Count == 1)
            {
                _logger.Information("Removing empty roll for lesson {lesson} at school {school}", lesson.Name, roll.SchoolCode);

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

            _logger.Information("Removing student {student} from lesson roll for {lesson} at school {school} due to moving schools", student.DisplayName, lesson.Name, notification.PreviousSchoolCode);

            updatedLessonIds.Add(lesson.Id);
        }

        List<SciencePracRoll> existingRollsAtNewSchool = lessons
            .Where(lesson => lesson.Offerings.Any(record => offeringIds.Contains(record.OfferingId)))
            .SelectMany(lesson => lesson.Rolls)
            .Where(roll =>
                roll.SchoolCode == notification.CurrentSchoolCode &&
                roll.Status == LessonStatus.Active)
            .ToList();

        foreach (SciencePracRoll roll in existingRollsAtNewSchool)
        {
            SciencePracLesson lesson = lessons.First(lesson => lesson.Id == roll.LessonId);

            _logger.Information("Adding student {student} to lesson roll for {lesson} at school {school} due to moving schools", student.DisplayName, lesson.Name, notification.CurrentSchoolCode);

            roll.AddStudent(notification.StudentId);

            updatedLessonIds.Remove(roll.LessonId);
        }

        if (updatedLessonIds.Count == 0)
        {
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.Information("Lesson rolls updated successfully for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.PreviousSchoolCode, notification.CurrentSchoolCode);

            return;
        }

        List<SciencePracLesson> newRolls = lessons
            .Where(lesson =>
                updatedLessonIds.Contains(lesson.Id) &&
                lesson.DueDate >= DateOnly.FromDateTime(DateTime.Today))
            .ToList();

        foreach (SciencePracLesson lesson in newRolls)
        {
            SciencePracRoll roll = new(
                lesson.Id,
                student.SchoolCode);

            roll.AddStudent(student.StudentId);

            lesson.AddRoll(roll);

            _logger.Information("Creating new roll for lesson {lesson} at school {school} as it did not already exist", lesson.Name, notification.CurrentSchoolCode);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Lesson rolls updated successfully for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.PreviousSchoolCode, notification.CurrentSchoolCode);
    }
}
