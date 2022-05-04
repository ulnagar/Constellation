using Constellation.Application.Features.Jobs.SentralFamilyDetailsSync.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.SentralFamilyDetailsSync.Notifications.FamilyWithoutValidEmailAddressFound
{
    public class SendEmailToSchoolAdministration : INotificationHandler<FamilyWithoutValidEmailAddressFoundNotification>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailGateway _emailSender;
        private readonly IRazorViewToStringRenderer _razorService;

        public SendEmailToSchoolAdministration(IAppDbContext context, IEmailGateway emailSender,
            IRazorViewToStringRenderer razorService)
        {
            _context = context;
            _emailSender = emailSender;
            _razorService = razorService;
        }

        public async Task Handle(FamilyWithoutValidEmailAddressFoundNotification notification, CancellationToken cancellationToken)
        {
            var family = await _context.StudentFamilies
                .Include(family => family.Students)
                .FirstOrDefaultAsync(family => family.Id == notification.FamilyId, cancellationToken);

            if (family == null)
            {
                return;
            }

            var students = family.Students.Where(student => !student.IsDeleted).ToList();
          
            var viewModel = "<p>Family email address for the following students is missing:</p><ul>";
            foreach (var student in students)
                viewModel += $"<li>{student.DisplayName}</li>";

            if (family.Parent1.EmailAddress != null)
                viewModel += $"<p>The email address was previously reported as {family.Parent1.EmailAddress}, but was not present on the most recent scan.";

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" }
            };

            await _emailSender.Send(toRecipients, null, null, "noreply@aurora.nsw.edu.au", "Student Family email address missing", body, null);
        }
    }
}
