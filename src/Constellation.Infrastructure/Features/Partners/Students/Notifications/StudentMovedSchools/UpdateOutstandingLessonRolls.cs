namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentMovedSchools;

using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.SciencePracs;
using MediatR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class UpdateOutstandingLessonRolls 
    : INotificationHandler<StudentMovedSchoolsNotification>
{
    private readonly ILogger _logger;
    private readonly IStudentRepository _studentRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOutstandingLessonRolls(
        IStudentRepository studentRepository,
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _logger = logger.ForContext<StudentMovedSchoolsNotification>();
        _studentRepository = studentRepository;
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StudentMovedSchoolsNotification notification, CancellationToken cancellationToken)
    {
        _logger.Information("Updating lesson rolls for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.OldSchoolCode, notification.NewSchoolCode);

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken); 

        List<OfferingId> offeringIds = student.Enrolments
            .Where(enrolment => !enrolment.IsDeleted)
            .Select(enrolments => enrolments.OfferingId)
            .Distinct()
            .ToList();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForStudent(notification.StudentId, cancellationToken);

        List<SciencePracRoll> oldRolls = lessons
            .SelectMany(lesson => lesson.Rolls)
            .Where(roll =>
                roll.SchoolCode == notification.OldSchoolCode &&
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

                lesson.Cancel();
            }

            roll.RemoveStudent(notification.StudentId);
            _logger.Information("Removing student {student} from lesson roll for {lesson} at school {school} due to moving schools", student.DisplayName, lesson.Name, notification.OldSchoolCode);

            updatedLessonIds.Add(lesson.Id);
        }

        List<SciencePracRoll> existingRollsAtNewSchool = lessons
            .Where(lesson => lesson.Offerings.Any(record => offeringIds.Contains(record.OfferingId)))
            .SelectMany(lesson => lesson.Rolls)
            .Where(roll => 
                roll.SchoolCode == notification.NewSchoolCode &&
                roll.Status == LessonStatus.Active)
            .ToList();

        foreach (SciencePracRoll roll in existingRollsAtNewSchool)
        {
            SciencePracLesson lesson = lessons.First(lesson => lesson.Id == roll.LessonId);

            _logger.Information("Adding student {student} to lesson roll for {lesson} at school {school} due to moving schools", student.DisplayName, lesson.Name, notification.NewSchoolCode);

            roll.AddStudent(notification.StudentId);

            updatedLessonIds.Remove(roll.LessonId);
        }

        if (updatedLessonIds.Count == 0)
        {
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.Information("Lesson rolls updated successfully for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.OldSchoolCode, notification.NewSchoolCode);

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

            _logger.Information("Creating new roll for lesson {lesson} at school {school} as it did not already exist", lesson.Name, notification.NewSchoolCode);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Lesson rolls updated successfully for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.OldSchoolCode, notification.NewSchoolCode);
    }
}
