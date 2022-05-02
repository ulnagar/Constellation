using Constellation.Application.Enums;
using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Notifications
{
    public class AddMSTeamOperation : MediatR.INotificationHandler<SchoolContactCreatedNotification>
    {
        private readonly IAppDbContext _context;

        public AddMSTeamOperation(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(SchoolContactCreatedNotification notification, CancellationToken cancellationToken)
        {
            // Validate entries
            var contact = await _context.SchoolContacts
                .Include(context => context.Assignments)
                .FirstOrDefaultAsync(contact => contact.Id == notification.Id);

            if (contact == null)
                return;

            var secondary = contact.Assignments
                .Any(a => a.School.Students.Any(s => s.CurrentGrade >= Core.Enums.Grade.Y07));

            var primary = contact.Assignments
                .Any(a => a.School.Students.Any(s => s.CurrentGrade <= Core.Enums.Grade.Y06));

            if (secondary)
            {
                var operation = new ContactAddedMSTeamOperation()
                {
                    ContactId = contact.Id,
                    DateScheduled = DateTime.Now,
                    TeamName = MicrosoftTeam.SecondaryPartnerSchools,
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Member
                };

                _context.Add(operation);
            }

            if (primary)
            {
                var operation = new ContactAddedMSTeamOperation()
                {
                    ContactId = contact.Id,
                    DateScheduled = DateTime.Now,
                    TeamName = MicrosoftTeam.PrimaryPartnerSchools,
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Member
                };

                _context.Add(operation);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
