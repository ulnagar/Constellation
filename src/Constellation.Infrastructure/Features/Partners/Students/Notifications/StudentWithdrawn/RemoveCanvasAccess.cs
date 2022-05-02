using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveCanvasAccess : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;

        public RemoveCanvasAccess(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            var operation = new DeleteUserCanvasOperation
            {
                UserId = notification.StudentId
            };

            _context.Add(operation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
