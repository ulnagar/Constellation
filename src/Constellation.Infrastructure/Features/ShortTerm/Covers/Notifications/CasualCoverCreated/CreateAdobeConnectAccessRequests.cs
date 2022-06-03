using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            foreach (var room in offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.RoomId).Distinct())
            {
                await _operationService.CreateCasualAdobeConnectAccess(cover.CasualId, room, cover.StartDate.AddDays(-1), cover.Id);
                await _operationService.RemoveCasualAdobeConnectAccess(cover.CasualId, room, cover.EndDate.AddDays(1), cover.Id);
            }

            await _operationService.CreateCasualMSTeamOwnerAccess(cover.CasualId, cover.OfferingId, cover.Id, cover.StartDate.AddDays(-1));
            await _operationService.RemoveCasualMSTeamAccess(cover.CasualId, cover.OfferingId, cover.Id, cover.EndDate.AddDays(1));

            // Add the Casual Coordinators to the Team as well
            if (!offering.Sessions.Any(session => session.StaffId == "1030937" && !session.IsDeleted)) //Cathy Crouch
            {
                await _operationService.CreateTeacherMSTeamOwnerAccess("1030937", cover.OfferingId, cover.StartDate.AddDays(-1), null);
                await _operationService.RemoveTeacherMSTeamAccess("1030937", cover.OfferingId, cover.EndDate.AddDays(1), null);
            }

            if (!offering.Sessions.Any(session => session.StaffId == "735422017" && !session.IsDeleted) && !offering.Course.Faculty.HasFlag(Faculty.Mathematics)) //Karen Bellamy
            {
                await _operationService.CreateTeacherMSTeamOwnerAccess("735422017", cover.OfferingId, cover.StartDate.AddDays(-1), null);
                await _operationService.RemoveTeacherMSTeamAccess("735422017", cover.OfferingId, cover.EndDate.AddDays(1), null);
            }
        }
    }
}
