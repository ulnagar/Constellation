using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Notifications.StaffCoverCreated
{
    public class CreateMicrosoftTeamsAccessRequests : INotificationHandler<StaffCoverCreatedNotification>
    {
        private readonly IAppDbContext _context;

        public CreateMicrosoftTeamsAccessRequests(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(StaffCoverCreatedNotification notification, CancellationToken cancellationToken)
        {
            var cover = (TeacherClassCover)await _context.Covers
                .FirstOrDefaultAsync(cover => cover.Id == notification.CoverId, cancellationToken);

            if (cover == null)
            {
                // Log error
                return;
            }

            var offering = await _context.Offerings
                .Include(offering => offering.Sessions)
                .Include(offering => offering.Course)
                .FirstOrDefaultAsync(offering => offering.Id == cover.OfferingId, cancellationToken);

            var casualAddOperation = new TeacherMSTeamOperation
            {
                OfferingId = offering.Id,
                StaffId = cover.StaffId,
                CoverId = cover.Id,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.StartDate.AddDays(-1)
            };

            var casualRemoveOperation = new TeacherMSTeamOperation
            {
                OfferingId = offering.Id,
                StaffId = cover.StaffId,
                CoverId = cover.Id,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = cover.EndDate.AddDays(1)
            };

            _context.Add(casualAddOperation);
            _context.Add(casualRemoveOperation);

            await _context.SaveChangesAsync(cancellationToken);

            // Add the Casual Coordinators to the Team as well
            if (!offering.Sessions.Any(session => session.StaffId == "1030937" && !session.IsDeleted)) //Cathy Crouch
            {
                var cathyAddOperation = new TeacherMSTeamOperation
                {
                    OfferingId = offering.Id,
                    StaffId = "1030937",
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = cover.StartDate.AddDays(-1),
                    CoverId = cover.Id
                };

                var cathyRemoveOperation = new TeacherMSTeamOperation
                {
                    OfferingId = offering.Id,
                    StaffId = "1030937",
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = cover.EndDate.AddDays(1),
                    CoverId = cover.Id
                };

                _context.Add(cathyAddOperation);
                _context.Add(cathyRemoveOperation);

                await _context.SaveChangesAsync(cancellationToken);
            }

            if (!offering.Sessions.Any(session => session.StaffId == "735422017" && !session.IsDeleted)) //Karen Bellamy
            {
                var karenAddOperation = new TeacherMSTeamOperation
                {
                    OfferingId = offering.Id,
                    StaffId = "735422017",
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = cover.StartDate.AddDays(-1),
                    CoverId = cover.Id
                };

                var karenRemoveOperation = new TeacherMSTeamOperation
                {
                    OfferingId = offering.Id,
                    StaffId = "735422017",
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                    DateScheduled = cover.EndDate.AddDays(1),
                    CoverId = cover.Id
                };

                _context.Add(karenAddOperation);
                _context.Add(karenRemoveOperation);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
