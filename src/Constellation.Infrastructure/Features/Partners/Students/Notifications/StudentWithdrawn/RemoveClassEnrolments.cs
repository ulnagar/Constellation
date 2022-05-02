using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Notifications.StudentWithdrawn
{
    public class RemoveClassEnrolments : INotificationHandler<StudentWithdrawnNotification>
    {
        private readonly IAppDbContext _context;

        public RemoveClassEnrolments(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(StudentWithdrawnNotification notification, CancellationToken cancellationToken)
        {
            using var dbContextTransaction = _context.Database.BeginTransactionAsync(cancellationToken);

            var enrolments = await _context.Enrolments
                .Where(enrolment => enrolment.StudentId == notification.StudentId && !enrolment.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var enrolment in enrolments)
            {
                enrolment.IsDeleted = true;

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
            }

            await _context.Database.CommitTransactionAsync(cancellationToken);
        }
    }
}
