using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveOutstandingLessonRolls : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public RemoveOutstandingLessonRolls(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger.ForContext<StudentWithdrawnNotification>();
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Attempting to remove student ({studentId}) from outstanding lesson rolls due to withdrawal event.", notification.StudentId);
            using var dbContextTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var lessons = await _context.LessonRolls
                .Include(roll => roll.Attendance)
                .Include(roll => roll.Lesson)
                .Where(roll => roll.Attendance.Any(student => student.StudentId == notification.StudentId) && roll.Status == Core.Enums.LessonStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var lesson in lessons)
            {
                var attendance = lesson.Attendance.First(student => student.StudentId == notification.StudentId);

                lesson.Attendance.Remove(attendance);

                _logger.Information("Removed student from lesson {lessonName}", lesson.Lesson.Name);

                if (lesson.Attendance.Count < 1)
                {
                    _logger.Information("Removing empty roll for lesson {lesson} at school {school}", lesson.Lesson.Name, lesson.SchoolCode);
                    _context.Remove(lesson);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            await dbContextTransaction.CommitAsync(cancellationToken);
            _logger.Information("Finished removing student ({studentId}) from outstanding lesson rolls", notification.StudentId);
        }
    }
}
