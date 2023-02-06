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

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Notifications.CasualCoverCancelled
{
    public class CancelMicrosoftTeamsAccessRequests : INotificationHandler<CasualCoverCancelledNotification>
    {
        private readonly IAppDbContext _context;

        public CancelMicrosoftTeamsAccessRequests(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(CasualCoverCancelledNotification notification, CancellationToken cancellationToken)
        {
            var cover = (CasualClassCover)await _context.Covers
                .FirstOrDefaultAsync(cover => cover.Id == notification.CoverId, cancellationToken);

            if (cover == null)
            {
                // Log error
                return;
            }

            var existingRequests = await _context.MSTeamOperations
                .Where(cover => cover.Id == notification.CoverId)
                .ToListAsync();

            var casualOperations = existingRequests
                .Where(operation => operation.GetType() == typeof(CasualMSTeamOperation))
                .ToList();

            var teacherOperations = existingRequests
                .Where(operation => operation.GetType() == typeof(TeacherMSTeamOperation))
                .GroupBy(operation => ((TeacherMSTeamOperation)operation).StaffId)
                .ToList();

            if (casualOperations.Any(operation => operation.Action == MSTeamOperationAction.Add && operation.IsCompleted) &&
                casualOperations.Any(operation => operation.Action == MSTeamOperationAction.Remove && !operation.IsCompleted))
            {
                // Access has been granted and must be removed
                var removeOperation = casualOperations.FirstOrDefault(operation => operation.Action == MSTeamOperationAction.Remove);

                if (removeOperation != null && !removeOperation.IsCompleted)
                {
                    removeOperation.DateScheduled = DateTime.Now;
                }
            }
            else
            {
                foreach (var operation in casualOperations)
                    operation.IsDeleted = true;
            }

            foreach (var group in teacherOperations)
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
