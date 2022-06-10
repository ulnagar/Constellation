using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Notifications.CasualCoverCreated
{
    public class CreateAdobeConnectAccessRequests : INotificationHandler<CasualCoverCreatedNotification>
    {
        private readonly IAppDbContext _context;

        public CreateAdobeConnectAccessRequests(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(CasualCoverCreatedNotification notification, CancellationToken cancellationToken)
        {
            var cover = (CasualClassCover)await _context.Covers
                .FirstOrDefaultAsync(cover => cover.Id == notification.CoverId, cancellationToken);

            if (cover == null)
            {
                // Log error
                return;
            }

            var rooms = await _context.Offerings
                .Where(offering => offering.Id == cover.OfferingId)
                .SelectMany(offering => offering.Sessions.Where(session => !session.IsDeleted))
                .Select(session => session.Room)
                .Distinct()
                .ToListAsync(cancellationToken);
                
            foreach (var room in rooms)
            {
                var addOperation = new CasualAdobeConnectOperation
                {
                    ScoId = room.ScoId,
                    CasualId = cover.CasualId,
                    Action = AdobeConnectOperationAction.Add,
                    DateScheduled = cover.StartDate.AddDays(-1),
                    CoverId = cover.Id
                };

                var removeOperation = new CasualAdobeConnectOperation
                {
                    ScoId = room.ScoId,
                    CasualId = cover.CasualId,
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = cover.EndDate.AddDays(1),
                    CoverId = cover.Id
                };

                _context.Add(addOperation );
                _context.Add(removeOperation);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
