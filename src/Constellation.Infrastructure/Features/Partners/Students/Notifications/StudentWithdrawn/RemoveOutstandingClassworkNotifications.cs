using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveOutstandingClassworkNotifications : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;

        public RemoveOutstandingClassworkNotifications(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            var missedWorkNotifications = await _context.ClassworkNotifications
                .Include(missedWork => missedWork.Absences)
                .Where(missedWork => missedWork.Absences.Any(absence => absence.StudentId == notification.StudentId) && !missedWork.CompletedAt.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var missedWork in missedWorkNotifications)
            {
                var absence = missedWork.Absences.First(absence => absence.StudentId == notification.StudentId);

                missedWork.Absences.Remove(absence);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
