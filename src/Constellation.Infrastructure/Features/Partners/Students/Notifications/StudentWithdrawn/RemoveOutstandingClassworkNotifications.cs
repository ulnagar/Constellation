namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn;

using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.MissedWork;
using MediatR;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class RemoveOutstandingClassworkNotifications : INotificationHandler<StudentWithdrawnNotification>
{
    private readonly IClassworkNotificationRepository _classworkNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveOutstandingClassworkNotifications(
        IClassworkNotificationRepository classworkNotificationRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _classworkNotificationRepository = classworkNotificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<StudentWithdrawnNotification>();
    }

    public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to remove student {studentId} from outstanding classwork notifications due to withdrawal", notification.StudentId);

        List<ClassworkNotification> notifications = await _classworkNotificationRepository.GetOutstandingForStudent(notification.StudentId, cancellationToken);

        foreach (ClassworkNotification missedWork in notifications)
        {
            Absence absence = missedWork
                .Absences
                .First(absence => absence.StudentId == notification.StudentId);

            missedWork.RemoveAbsence(absence);

            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.Information("Removed student {studentId} from classwork notification ({class} @ {date})", notification.StudentId, missedWork.OfferingId, missedWork.AbsenceDate.ToShortDateString());
        }
    }
}
