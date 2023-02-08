using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveClassEnrolments : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;
        private readonly ILogger _logger;

        public RemoveClassEnrolments(IAppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger.ForContext<StudentWithdrawnNotification>();
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            using var dbContextTransaction = _context.Database.BeginTransactionAsync(cancellationToken);

            _logger.Information("Attempting to unenroll student {studentId} from classes due to withdrawal", notification.StudentId);

            var enrolments = await _context.Enrolments
                .Include(enrolment => enrolment.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Room)
                .Where(enrolment => enrolment.StudentId == notification.StudentId && !enrolment.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var enrolment in enrolments)
            {
                enrolment.IsDeleted = true;
                enrolment.DateDeleted = DateTime.Now;

                //Remove Class Specific Adobe Connect Access
                foreach (var room in enrolment.Offering.Sessions.Select(session => session.Room).Distinct().ToList())
                {
                    var adobeOperation = new StudentAdobeConnectOperation
                    {
                        ScoId = room.ScoId,
                        StudentId = notification.StudentId,
                        Action = AdobeConnectOperationAction.Remove,
                        DateScheduled = DateTime.Now
                    };

                    _context.Add(adobeOperation);
                }

                //Remove Class Specific MS Team Access
                var teamsOperation = new StudentMSTeamOperation
                {
                    StudentId = notification.StudentId,
                    OfferingId = enrolment.OfferingId,
                    DateScheduled = DateTime.Now,
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Member
                };

                _context.Add(teamsOperation);

                //Remove Class Specific Canvas Access
                var canvasOperation = new ModifyEnrolmentCanvasOperation
                {
                    UserId = notification.StudentId,
                    CourseId = $"{enrolment.Offering.EndDate.Year}-{enrolment.Offering.Name[0..^1]}",
                    Action = CanvasOperation.EnrolmentAction.Remove,
                    ScheduledFor = DateTime.Now
                };

                _context.Add(canvasOperation);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.Information("Student {studentId} removed from class {class}", notification.StudentId, enrolment.Offering.Name);
            }

            await _context.Database.CommitTransactionAsync(cancellationToken);
        }
    }
}
