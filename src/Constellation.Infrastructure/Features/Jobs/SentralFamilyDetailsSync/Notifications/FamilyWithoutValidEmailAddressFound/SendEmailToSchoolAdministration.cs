using Constellation.Application.Features.Jobs.SentralFamilyDetailsSync.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
        private readonly ILogger _logger;

        public SendEmailToSchoolAdministration(IAppDbContext context, IEmailGateway emailSender,
            IRazorViewToStringRenderer razorService, ILogger logger)
        {
            _context = context;
            _emailSender = emailSender;
            _razorService = razorService;
            _logger = logger.ForContext<FamilyWithoutValidEmailAddressFoundNotification>();
        }

        public async Task Handle(FamilyWithoutValidEmailAddressFoundNotification notification, CancellationToken cancellationToken)
        {
            _logger.Information("Detected family {familyId} without email address. Sending notification to school admin.", notification.FamilyId);
            
            var family = await _context.StudentFamilies
                .Include(family => family.Students)
                .FirstOrDefaultAsync(family => family.Id == notification.FamilyId, cancellationToken);

            if (family == null)
            {
                _logger.Warning("Could not find family {familyId} in database.", notification.FamilyId);

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
                { "Aurora College IT Support", "auroracollegeitsupport@det.nsw.edu.au" },
                { "Aurora College", "auroracoll-h.school@det.nsw.edu.au" }
            };

            await _emailSender.Send(toRecipients, null, null, "noreply@aurora.nsw.edu.au", "Student Family email address missing", body, null);

            _logger.Information("Email sent to school admin regarding missing email address for {family}.", family.Address.Title);
        }
    }
}
