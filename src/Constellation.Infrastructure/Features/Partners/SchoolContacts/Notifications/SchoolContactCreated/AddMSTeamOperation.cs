using Constellation.Application.Enums;
using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Notifications
{
    public class AddMSTeamOperation : INotificationHandler<SchoolContactCreatedNotification>
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
                .Select(entry => 
                    new ContactWithAssignmentSpecifications 
                    { 
                        Id = entry.Id, 
                        IsPrimary = entry.Assignments.Any(assignment => !assignment.IsDeleted && assignment.School.Students.Any(student => !student.IsDeleted && student.CurrentGrade <= Grade.Y06)),
                        IsSecondary = entry.Assignments.Any(assignment => !assignment.IsDeleted && assignment.School.Students.Any(student => !student.IsDeleted && student.CurrentGrade >= Grade.Y07))
                    })
                .FirstOrDefaultAsync(contact => contact.Id == notification.Id, cancellationToken);

            if (contact == null)
                return;

            if (contact.IsSecondary)
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

            if (contact.IsPrimary)
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

        private class ContactWithAssignmentSpecifications
        {
            public int Id { get; set; }
            public bool IsPrimary { get; set; }
            public bool IsSecondary { get; set; }
        }
    }
}
