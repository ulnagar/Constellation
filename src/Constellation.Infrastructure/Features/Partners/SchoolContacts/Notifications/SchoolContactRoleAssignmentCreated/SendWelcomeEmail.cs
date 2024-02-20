using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Infrastructure.Templates.Views.Emails.Contacts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Notifications.SchoolContactRoleAssignmentCreated
{
    public class SendWelcomeEmail : INotificationHandler<SchoolContactRoleAssignmentCreatedNotification>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailGateway _emailSender;
        private readonly IRazorViewToStringRenderer _razorService;

        public SendWelcomeEmail(IAppDbContext context, IEmailGateway emailSender, IRazorViewToStringRenderer razorService)
        {
            _context = context;
            _emailSender = emailSender;
            _razorService = razorService;
        }

        public async Task Handle(SchoolContactRoleAssignmentCreatedNotification notification, CancellationToken cancellationToken)
        {
            // Validate entries
            var assignment = await _context.SchoolContactRoles
                .Include(assignment => assignment.SchoolContact)
                .Include(assignment => assignment.School)
                .FirstOrDefaultAsync(assignment => assignment.Id == notification.AssignmentId);

            if (assignment == null)
                return;

            if (assignment.Role == SchoolContactRole.Coordinator)
            {
                // Send ACC email
                var viewModel = new NewACCoordinatorEmailViewModel
                {
                    Title = $"Welcome to Aurora College!",
                    SenderName = "Virginia Cluff",
                    SenderTitle = "Instructional Leader",
                    Preheader = "",
                    PartnerSchool = assignment.School.Name
                };

                var toRecipients = new Dictionary<string, string>
                {
                    { assignment.SchoolContact.DisplayName, assignment.SchoolContact.EmailAddress }
                };

                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewACCoordinatorEmail.cshtml", viewModel);

                await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body, cancellationToken);
            }

            if (assignment.Role == SchoolContactRole.SciencePrac)
            {
                // Send SPT email
                var viewModel = new NewSciencePracTeacherEmailViewModel
                {
                    Title = $"Welcome to Aurora College!",
                    SenderName = "Fiona Boneham",
                    SenderTitle = "Science Practical Coordinator",
                    Preheader = "",
                    PartnerSchool = assignment.School.Name
                };

                var toRecipients = new Dictionary<string, string>
                {
                    { assignment.SchoolContact.DisplayName, assignment.SchoolContact.EmailAddress }
                };

                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewSciencePracTeacherEmail.cshtml", viewModel);

                await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body, cancellationToken);
            }
        }
    }
}
