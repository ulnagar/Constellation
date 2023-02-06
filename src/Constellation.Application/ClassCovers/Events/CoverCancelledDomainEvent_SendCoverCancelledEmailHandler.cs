using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.ClassCovers.Events;

internal sealed class CoverCancelledDomainEvent_SendCoverCancelledEmailHandler
    : IDomainEventHandler<CoverCancelledDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailGateway _emailGateway;
    private readonly IRazorViewToStringRenderer _razorViewRenderer;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger _logger;

    public CoverCancelledDomainEvent_SendCoverCancelledEmailHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository,
        ICourseOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IEmailGateway emailGateway,
        IRazorViewToStringRenderer razorViewRenderer,
        ITeamRepository teamRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _emailGateway = emailGateway;
        _razorViewRenderer = razorViewRenderer;
        _teamRepository = teamRepository;
        _logger = logger.ForContext<CoverCancelledDomainEvent>();
    }

    public async Task Handle(CoverCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        var offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

        var primaryRecipients = new List<EmailAddress>(); // Casual, Classroom Teacher
        var secondaryRecipients = new List<EmailAddress>(); // Head Teacher, Additional Recipients

        var teachers = await _staffRepository.GetCurrentTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (var teacher in teachers)
        {
            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailAddress.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCancelledDomainEvent_SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);

                    continue;
                }

                primaryRecipients.Add(address.Value);
            }
        }
            
        var casual = await _context.Casuals
            .FirstOrDefaultAsync(casual => casual.Id == cover.CasualId, cancellationToken);

        

        var headTeachers = await _context.Faculties
            .Where(faculty => faculty.Id == offering.Course.FacultyId)
                .SelectMany(faculty => faculty.Members)
            .Where(member => member.Role == FacultyMembershipRole.Manager && !member.IsDeleted)
                .Select(member => member.Staff)
                .ToListAsync(cancellationToken);

        var additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

        var teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(), cancellationToken);

        

        primaryRecipients.Add(casual.DisplayName, casual.EmailAddress);

        foreach (var teacher in teachers)
        {
            if (!primaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress) && !secondaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress))
                secondaryRecipients.Add(teacher.DisplayName, teacher.EmailAddress);
        }

        foreach (var teacher in headTeachers)
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
            ToName = casual.DisplayName,
            Title = $"Cancelled Aurora Class Cover - {offering.Name}",
            SenderName = "Cathy Crouch",
            SenderTitle = "Casual Coordinator",
            StartDate = cover.StartDate,
            EndDate = cover.EndDate,
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            var body = await _razorViewRenderer.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            var uid = $"{cover.Id}-{cover.OfferingId}-{cover.StartDate:yyyyMMdd}";
            var summary = $"Aurora College Cover - {cover.Offering.Name}";
            var location = $"Class Team ({teamLink}";
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

            var icsData = _calendarService.CancelInvite(uid, casual.DisplayName, casual.EmailAddress, summary, location, description, appointmentStart, appointmentEnd, 0);

            await _emailGateway.Send(primaryRecipients, secondaryRecipients, "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, icsData);
        }
        else
        {
            var body = await _razorViewRenderer.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverEmail.cshtml", viewModel);

            await _emailGateway.Send(primaryRecipients, secondaryRecipients, "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments);
        }
    }
}
