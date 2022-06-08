using Constellation.Application.Extensions;
using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Infrastructure.Templates.Views.Emails.Covers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Notifications.StaffCoverCancelled
{
    public class SendCoverCancellationEmail : INotificationHandler<StaffCoverCancelledNotification>
    {
        private readonly IAppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRazorViewToStringRenderer _razorService;
        private readonly IEmailGateway _emailGateway;
        private readonly ICalendarService _calendarService;

        public SendCoverCancellationEmail(IAppDbContext context, UserManager<AppUser> userManager,
            IRazorViewToStringRenderer razorService, IEmailGateway emailGateway,
            ICalendarService calendarService)
        {
            _context = context;
            _userManager = userManager;
            _razorService = razorService;
            _emailGateway = emailGateway;
            _calendarService = calendarService;
        }

        public async Task Handle(StaffCoverCancelledNotification notification, CancellationToken cancellationToken)
        {
            // Gather details
            var cover = await _context.Covers
                .FirstOrDefaultAsync(cover => cover.Id == notification.CoverId, cancellationToken) as TeacherClassCover;

            var offering = await _context.Offerings
                .FirstOrDefaultAsync(offering => offering.Id == cover.OfferingId, cancellationToken);

            var staff = await _context.Staff
                .FirstOrDefaultAsync(staff => staff.StaffId == cover.StaffId, cancellationToken);

            var teachers = await _context.Sessions
                .Where(session => !session.IsDeleted && session.OfferingId == cover.OfferingId)
                .Select(session => session.Teacher)
                .ToListAsync(cancellationToken);

            var headTeacher = await _context.Courses
                .Where(course => course.Offerings.Any(offering => offering.Id == cover.OfferingId))
                .Select(course => course.HeadTeacher)
                .ToListAsync(cancellationToken);

            var additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

            var teamLink = await _context.Teams
                .FirstOrDefaultAsync(team => team.Name.Contains(offering.Name) && team.Name.Contains(cover.EndDate.Year.ToString()), cancellationToken);

            var primaryRecipients = new Dictionary<string, string>(); // Casual, Classroom Teacher
            var secondaryRecipients = new Dictionary<string, string>(); // Head Teacher, Additional Recipients

            primaryRecipients.Add(staff.DisplayName, staff.EmailAddress);

            foreach (var teacher in teachers)
            {
                if (!primaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress))
                    primaryRecipients.Add(teacher.DisplayName, teacher.EmailAddress);
            }

            foreach (var teacher in headTeacher)
            {
                if (!primaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress) && !secondaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress))
                    secondaryRecipients.Add(teacher.DisplayName, teacher.EmailAddress);
            }

            foreach (var user in additionalRecipients)
            {
                if (!primaryRecipients.Any(recipient => recipient.Value == user.Email) && !secondaryRecipients.Any(recipient => recipient.Value == user.Email))
                    secondaryRecipients.Add(user.DisplayName, user.Email);
            }

            // Determine whether email or invite
            var singleDayCover = cover.StartDate == cover.EndDate;

            // Prepare attachments
            var attachments = new List<Attachment>();

            // Send
            var viewModel = new CancelledCoverEmailViewModel
            {
                ToName = staff.DisplayName,
                Title = $"Aurora Class Cover - {offering.Name}",
                SenderName = "Ben Hillsley",
                SenderTitle = "Learning Technologies Support Officer",
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,
                HasAdobeAccount = true,
                Preheader = "",
                ClassWithLink = new Dictionary<string, string> { { teamLink.Name, teamLink.Link } }
            };

            if (singleDayCover)
            {
                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverAppointment.cshtml", viewModel);

                // Create and add ICS files
                var uid = $"{cover.Id}-{cover.Offering.Id}-{cover.StartDate:yyyyMMdd}";
                var summary = $"Aurora College Cover - {cover.Offering.Name}";
                var location = $"{teamLink.Name} ({teamLink.Link}";
                var description = body;

                // What cycle day does the cover fall on?
                // What periods exist for this class on that cycle day?
                // Extract start and end times for the periods to use in the appointment
                var cycleDay = cover.StartDate.GetDayNumber();
                var periods = await _context.Periods
                    .Where(period => period.Day == cycleDay && period.OfferingSessions.Any(session => !session.IsDeleted && session.OfferingId == cover.OfferingId))
                    .ToListAsync(cancellationToken);
                var appointmentStartTime = periods.Min(period => period.StartTime);
                var appointmentEndTime = periods.Max(period => period.EndTime);

                var appointmentStart = cover.StartDate.Add(appointmentStartTime);
                var appointmentEnd = cover.EndDate.Add(appointmentEndTime);

                var icsData = _calendarService.CancelInvite(uid, staff.DisplayName, staff.EmailAddress, summary, location, description, appointmentStart, appointmentEnd, 0);

                await _emailGateway.Send(new Dictionary<string, string> { { "Ben Hillsley", "benjamin.hillsley@det.nsw.edu.au" } }, "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, icsData);
            }
            else
            {
                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverEmail.cshtml", viewModel);

                await _emailGateway.Send(new Dictionary<string, string> { { "Ben Hillsley", "benjamin.hillsley@det.nsw.edu.au" } }, "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments);
            }
        }
    }
}
