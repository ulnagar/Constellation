using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.Templates.Views.Emails.Contacts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Notifications
{
    public class SendWelcomeEmail : INotificationHandler<SchoolContactCreatedNotification>
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

        public async Task Handle(SchoolContactCreatedNotification notification, CancellationToken cancellationToken)
        {
            // Validate entries
            var contact = await _context.SchoolContacts
                .Include(context => context.Assignments)
                .ThenInclude(assignment => assignment.School)
                .FirstOrDefaultAsync(contact => contact.Id == notification.Id);

            if (contact == null || contact.Assignments.Count == 0 || contact.Assignments.All(assignment => assignment.IsDeleted))
                return;

            var assignment = contact.Assignments.Aggregate((item1, item2) => item1.DateEntered > item2.DateEntered ? item1 : item2);

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
                    { contact.DisplayName, contact.EmailAddress }
                };

                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewACCoordinatorEmail.cshtml", viewModel);

                await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
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
                    { contact.DisplayName, contact.EmailAddress }
                };

                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewSciencePracTeacherEmail.cshtml", viewModel);

                await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
            }
        }
    }
}
