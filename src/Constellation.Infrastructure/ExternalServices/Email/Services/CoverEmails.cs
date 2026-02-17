namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.AppSettings.Models;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Covers;
using Constellation.Infrastructure.Templates.Views.Emails.Covers;
using Core.Errors;
using Core.Models.Offerings;
using Core.Shared;
using Core.ValueObjects;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendNewCoverEmail(
    Cover cover,
    Offering offering,
    EmailRecipient coveringTeacher,
    List<EmailRecipient> primaryRecipients,
    List<EmailRecipient> secondaryRecipients,
    TimeOnly startTime,
    TimeOnly endTime,
    string teamLink,
    List<Attachment> attachments,
    CancellationToken cancellationToken = default)
    {
        // Determine whether email or invite
        bool singleDayCover = cover.StartDate == cover.EndDate;

        CoversConfiguration? configuration = await _appSettings.Covers(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendNewCoverEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(CoversConfiguration)), true)
                .Warning("Failed to send cover email");

            return;
        }

        // Send
        NewCoverEmailViewModel viewModel = new()
        {
            ContactName = configuration.ContactName,
            ContactPhone = configuration.ContactPhone,
            ToName = coveringTeacher.Name,
            Title = $"Aurora Class Cover - {offering.Name}",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            StartDate = cover.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = cover.EndDate.ToDateTime(TimeOnly.MinValue),
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            string uid = $"{cover.Id}-{cover.OfferingId}-{cover.StartDate:yyyyMMdd}";
            string summary = $"Aurora College Cover - {offering.Name}";
            string location = $"Class Team ({teamLink})";

            // What cycle day does the cover fall on?
            // What periods exist for this class on that cycle day?
            // Extract start and end times for the periods to use in the appointment
            DateTime appointmentStart = cover.StartDate.ToDateTime(startTime);
            DateTime appointmentEnd = cover.EndDate.ToDateTime(endTime);

            string icsData = _calendarService.CreateInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, body, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, icsData, MessagePriority.Normal, cancellationToken);
        }
        else
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, MessagePriority.Normal, cancellationToken);
        }

    }

    public async Task SendUpdatedCoverEmail(
        Cover cover,
        Offering offering,
        EmailRecipient coveringTeacher,
        List<EmailRecipient> primaryRecipients,
        List<EmailRecipient> secondaryRecipients,
        DateOnly originalStartDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string teamLink,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        // Determine whether email or invite
        bool singleDayCover = cover.StartDate == cover.EndDate;

        CoversConfiguration? configuration = await _appSettings.Covers(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendNewCoverEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(CoversConfiguration)), true)
                .Warning("Failed to send cover email");

            return;
        }

        UpdatedCoverEmailViewModel viewModel = new()
        {
            ContactName = configuration.ContactName,
            ContactPhone = configuration.ContactPhone,
            ToName = coveringTeacher.Name,
            Title = $"[UPDATED] Aurora Class Cover - {offering.Name}",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            StartDate = cover.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = cover.EndDate.ToDateTime(TimeOnly.MinValue),
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            string uid = $"{cover.Id}-{cover.OfferingId}-{originalStartDate:yyyyMMdd}";
            string summary = $"[UPDATED] Aurora Class Cover - {offering.Name}";
            string location = $"Class Team ({teamLink})";

            // What cycle day does the cover fall on?
            // What periods exist for this class on that cycle day?
            // Extract start and end times for the periods to use in the appointment
            DateTime appointmentStart = cover.StartDate.ToDateTime(startTime);
            DateTime appointmentEnd = cover.EndDate.ToDateTime(endTime);

            string icsData = _calendarService.CreateInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, body, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, icsData, MessagePriority.Normal, cancellationToken);
        }
        else
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, MessagePriority.Normal, cancellationToken);
        }
    }

    public async Task SendCancelledCoverEmail(
        Cover cover,
        Offering offering,
        EmailRecipient coveringTeacher,
        List<EmailRecipient> primaryRecipients,
        List<EmailRecipient> secondaryRecipients,
        TimeOnly startTime,
        TimeOnly endTime,
        string teamLink,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        // Determine whether email or invite
        bool singleDayCover = cover.StartDate == cover.EndDate;

        CoversConfiguration? configuration = await _appSettings.Covers(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendNewCoverEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(CoversConfiguration)), true)
                .Warning("Failed to send cover email");

            return;
        }

        // Send
        CancelledCoverEmailViewModel viewModel = new()
        {
            ContactName = configuration.ContactName,
            ContactPhone = configuration.ContactPhone,
            ToName = coveringTeacher.Name,
            Title = $"Cancelled Aurora Class Cover - {offering.Name}",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            StartDate = cover.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = cover.EndDate.ToDateTime(TimeOnly.MinValue),
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            string uid = $"{cover.Id}-{cover.OfferingId}-{cover.StartDate:yyyyMMdd}";
            string summary = $"Aurora College Cover - {offering.Name}";
            string location = $"Class Team ({teamLink}";

            DateTime appointmentStart = cover.StartDate.ToDateTime(startTime);
            DateTime appointmentEnd = cover.EndDate.ToDateTime(endTime);
            string icsData = _calendarService.CancelInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, body, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, icsData, MessagePriority.Normal, cancellationToken);
        }
        else
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, MessagePriority.Normal, cancellationToken);
        }
    }
}
