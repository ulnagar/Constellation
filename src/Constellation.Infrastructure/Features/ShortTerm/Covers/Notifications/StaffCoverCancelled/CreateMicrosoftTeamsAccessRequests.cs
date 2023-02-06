using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Covers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Notifications.StaffCoverCancelled
{
    public class CancelMicrosoftTeamsAccessRequests : INotificationHandler<StaffCoverCancelledNotification>
    {
        private readonly IAppDbContext _context;

        public CancelMicrosoftTeamsAccessRequests(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(StaffCoverCancelledNotification notification, CancellationToken cancellationToken)
        {
            var cover = (TeacherClassCover)await _context.Covers
                .FirstOrDefaultAsync(cover => cover.Id == notification.CoverId, cancellationToken);

            if (cover == null)
            {
                // Log error
                return;
            }

            var existingRequests = await _context.MSTeamOperations
                .OfType<TeacherMSTeamOperation>()
                .Where(operation => operation.Id == notification.CoverId)
                .ToListAsync(cancellationToken);

            var groupedRequests = existingRequests.GroupBy(operation => operation.StaffId).ToList();

            foreach (var group in groupedRequests)
            {
                if (group.Any(operation => operation.Action == MSTeamOperationAction.Add && operation.IsCompleted) &&
                    group.Any(operation => operation.Action == MSTeamOperationAction.Remove && !operation.IsCompleted))
                {
                    // Access has been granted and must be removed
                    var removeOperation = group.FirstOrDefault(operation => operation.Action == MSTeamOperationAction.Remove);

                    if (removeOperation != null && !removeOperation.IsCompleted)
                    {
                        removeOperation.DateScheduled = DateTime.Now;
                    }
                }
                else
                {
                    foreach (var operation in group)
                        operation.IsDeleted = true;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
