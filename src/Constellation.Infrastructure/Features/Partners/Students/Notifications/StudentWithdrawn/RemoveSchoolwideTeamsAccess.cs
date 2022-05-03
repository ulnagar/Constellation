using Constellation.Application.Enums;
using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveSchoolwideTeamsAccess : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public RemoveSchoolwideTeamsAccess(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger.ForContext<StudentWithdrawnNotification>();
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Attempting to remove student ({studentId}) from school wide teams due to withdrawal", notification.StudentId);

            var student = await _context.Students
                .FirstOrDefaultAsync(student => student.StudentId == notification.StudentId, cancellationToken);

            if (student == null)
            {
                _logger.Warning("Could not find student with Id {studentId} to remove from school wide teams", notification.StudentId);
                return;
            }

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

            _logger.Information("Scheduled student ({studentId}) removal from school wide teams due to withdrawal", notification.StudentId);
        }
    }
}
