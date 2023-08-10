namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn;

using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models.SciencePracs;
using MediatR;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class RemoveOutstandingLessonRolls 
    : INotificationHandler<StudentWithdrawnNotification>
{
    private readonly ILogger _logger;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveOutstandingLessonRolls(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _logger = logger.ForContext<StudentWithdrawnNotification>();
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to remove student ({studentId}) from outstanding lesson rolls due to withdrawal event.", notification.StudentId);

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForStudent(notification.StudentId);

        List<SciencePracRoll> rolls = lessons
            .SelectMany(lesson =>
                lesson.Rolls.Where(roll =>
                    roll.Attendance.Any(attendance =>
                        attendance.StudentId == notification.StudentId) &&
                    roll.Status == LessonStatus.Active))
            .ToList();

        foreach (SciencePracRoll roll in rolls)
        {
            SciencePracLesson lesson = lessons.First(lesson => lesson.Id == roll.LessonId);

            if (roll.Attendance.Count == 1)
            {
                _logger.Information("Removing empty roll for lesson {lesson} at school {school}", lesson.Name, roll.SchoolCode);

                lesson.Cancel();
            }

            roll.RemoveStudent(notification.StudentId);
            _logger.Information("Removed student from lesson {lessonName}", lesson.Name);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Finished removing student ({studentId}) from outstanding lesson rolls", notification.StudentId);
    }
}
