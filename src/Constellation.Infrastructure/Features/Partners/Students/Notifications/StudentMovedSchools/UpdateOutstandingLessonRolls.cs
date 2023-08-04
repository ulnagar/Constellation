using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.SciencePracs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentMovedSchools
{
    public class UpdateOutstandingLessonRolls : INotificationHandler<StudentMovedSchoolsNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public UpdateOutstandingLessonRolls(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger.ForContext<StudentMovedSchoolsNotification>();
        }

        public async Task Handle(StudentMovedSchoolsNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Updating lesson rolls for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.OldSchoolCode, notification.NewSchoolCode);

            var student = await _context.Students
                .Include(student => student.Enrolments)
                .ThenInclude(enrolment => enrolment.Offering)
                .FirstOrDefaultAsync(student => student.StudentId == notification.StudentId, cancellationToken);

            var activeEnrolments = student.Enrolments
                .Where(enrolment => !enrolment.IsDeleted)
                .Select(enrolments => enrolments.OfferingId)
                .Distinct()
                .ToList();

            var oldRolls = await _context.LessonRolls
                .Include(roll => roll.Attendance)
                .Include(roll => roll.Lesson)
                .Where(roll => roll.SchoolCode == notification.OldSchoolCode &&
                    roll.Status == LessonStatus.Active && 
                    roll.Attendance.Any(attendance => attendance.StudentId == notification.StudentId))
                .ToListAsync(cancellationToken);

            foreach (var roll in oldRolls)
            {
                var attendance = roll.Attendance.First(attendance => attendance.StudentId == notification.StudentId);

                _context.Remove(attendance);

                _logger.Information("Removing student {student} from lesson roll for {lesson} at school {school} due to moving schools", student.DisplayName, roll.Lesson.Name, notification.OldSchoolCode);

                if (roll.Attendance.Count <= 1)
                {
                    _logger.Information("Removing empty roll for lesson {lesson} at school {school}", roll.Lesson.Name, notification.OldSchoolCode);
                    _context.Remove(roll);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            var existingRollsAtNewSchool = await _context.LessonRolls
                .Include(roll => roll.Lesson)
                .Include(roll => roll.Attendance)
                .Where(roll => roll.SchoolCode == notification.NewSchoolCode &&
                    roll.Status == LessonStatus.Active &&
                    roll.Lesson.Offerings.Any(offering => activeEnrolments.Contains(offering.Id)))
                .ToListAsync(cancellationToken);

            foreach (var roll in existingRollsAtNewSchool)
            {
                if (roll.Attendance.Any(Attendance => Attendance.StudentId == notification.StudentId))
                    continue;

                var attendance = new SciencePracRoll.LessonRollStudentAttendance
                {
                    LessonRollId = roll.Id,
                    StudentId = notification.StudentId
                };

                _context.Add(attendance);

                _logger.Information("Adding student {student} to lesson roll for {lesson} at school {school} due to moving schools", student.DisplayName, roll.Lesson.Name, notification.NewSchoolCode);

                await _context.SaveChangesAsync(cancellationToken);
            }

            var newRolls = await _context.Lessons
                .Where(lesson => lesson.Offerings.Any(offering => activeEnrolments.Contains(offering.Id)) &&
                    lesson.Rolls.All(roll => roll.SchoolCode != notification.NewSchoolCode) &&
                    lesson.DueDate >= DateTime.Now)
                .ToListAsync(cancellationToken);

            foreach (var lesson in newRolls)
            {
                var roll = new SciencePracAttendance
                {
                    LessonId = lesson.Id,
                    SchoolCode = notification.NewSchoolCode,
                    Status = LessonStatus.Active
                };

                roll.Attendance.Add(new SciencePracRoll.LessonRollStudentAttendance
                {
                    StudentId = notification.StudentId
                });

                _context.Add(roll);

                _logger.Information("Creating new roll for lesson {lesson} at school {school} as it did not already exist", lesson.Name, notification.NewSchoolCode);

                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Lesson rolls updated successfully for student ({studentId}) move notification from {oldSchool} to {newSchool}", notification.StudentId, notification.OldSchoolCode, notification.NewSchoolCode);
        }
    }
}
