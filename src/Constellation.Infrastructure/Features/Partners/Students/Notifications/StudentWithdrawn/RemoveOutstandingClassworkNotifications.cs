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
    public class RemoveOutstandingClassworkNotifications : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public RemoveOutstandingClassworkNotifications(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger.ForContext<StudentWithdrawnNotification>();
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Attempting to remove student {studentId} from outstanding classwork notifications due to withdrawal", notification.StudentId);

            var missedWorkNotifications = await _context.ClassworkNotifications
                .Include(missedWork => missedWork.Absences)
                .Include(missedWork => missedWork.Offering)
                .Where(missedWork => missedWork.Absences.Any(absence => absence.StudentId == notification.StudentId) && !missedWork.CompletedAt.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var missedWork in missedWorkNotifications)
            {
                var absence = missedWork.Absences.First(absence => absence.StudentId == notification.StudentId);

                missedWork.Absences.Remove(absence);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.Information("Removed student {studentId} from classwork notification ({class} @ {date})", notification.StudentId, missedWork.Offering.Name, missedWork.AbsenceDate.ToShortDateString());
            }
        }
    }
}
