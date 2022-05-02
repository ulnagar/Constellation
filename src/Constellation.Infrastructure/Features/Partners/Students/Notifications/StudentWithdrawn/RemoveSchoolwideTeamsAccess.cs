using Constellation.Application.Enums;
using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveSchoolwideTeamsAccess : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;

        public RemoveSchoolwideTeamsAccess(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(student => student.StudentId == notification.StudentId, cancellationToken);

            var operation = new StudentEnrolledMSTeamOperation
            {
                StudentId = notification.StudentId,
                TeamName = MicrosoftTeam.Students,
                DateScheduled = DateTime.Now,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _context.Add(operation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
