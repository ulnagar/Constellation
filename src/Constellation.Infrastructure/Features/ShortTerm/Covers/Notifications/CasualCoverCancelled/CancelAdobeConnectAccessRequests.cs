using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Notifications.CasualCoverCancelled
{
    public class CancelAdobeConnectAccessRequests : INotificationHandler<CasualCoverCancelledNotification>
    {
        private readonly IAppDbContext _context;

        public CancelAdobeConnectAccessRequests(IAppDbContext context)
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

            var existingRequests = await _context.AdobeConnectOperations
                .Where(operation => operation.CoverId == cover.Id)
                .ToListAsync();

            if (existingRequests.Any(operation => operation.Action == AdobeConnectOperationAction.Add && operation.IsCompleted) && 
                !existingRequests.Any(operation => operation.Action == AdobeConnectOperationAction.Remove && !operation.IsCompleted))
            {
                // Access has been granted and must be removed
                var removeOperation = existingRequests.FirstOrDefault(operation => operation.Action == AdobeConnectOperationAction.Remove);

                if (removeOperation != null && !removeOperation.IsCompleted)
                {
                    removeOperation.DateScheduled = DateTime.Now;
                }
            }
            else
            {
                foreach (var operation in existingRequests)
                    operation.IsDeleted = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
