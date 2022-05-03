using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveCanvasAccess : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public RemoveCanvasAccess(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger.ForContext<StudentWithdrawnNotification>();
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Attempting to remove Canvas access for student {studentId} due to withdrawal", notification.StudentId);

            var operation = new DeleteUserCanvasOperation
            {
                UserId = notification.StudentId
            };

            _context.Add(operation);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.Information("Scheduled removal of Canvas access for student {studentId} due to withdrawal", notification.StudentId);
        }
    }
}
