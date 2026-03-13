namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using Constellation.Core.Models.Messaging.Email;
using Constellation.Core.Models.Messaging.Email.Enums;
using Core.Shared;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendAdminAbsenceSentralAlert(string studentName)
    {
        string viewModel = $"<p>{studentName} cannot be located in the Sentral Users list and does not currently have a Sentral Student Id specified.</p>";

        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", viewModel);

        EmailMessage message = new()
        {
            From = EmailRecipient.NoReply,
            SendingModule = string.Empty,
            Subject = "[Aurora College] Student absence notification",
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        message.AddRecipient(EmailRecipient.InfoTechTeam, EmailRecipientType.To);
        
        await _emailSender.Send(message);
    }

    public async Task SendAdminAbsenceContactAlert(string studentName)
    {
        string viewModel = $"<p>Parent contact details for {studentName} cannot be located in Sentral.";

        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", viewModel);

        EmailMessage message = new()
        {
            From = EmailRecipient.NoReply,
            SendingModule = string.Empty,
            Subject = "[Aurora College] Constellation Data Issue Identified",
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        message.AddRecipient(EmailRecipient.InfoTechTeam, EmailRecipientType.To);

        await _emailSender.Send(message);
    }

    public async Task SendParentContactChangeReportEmail(
        MemoryStream report,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>Parent Contact Change Report for {DateTime.Today.ToLongDateString()} is attached.</p>";

        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", viewModel);

        EmailMessage message = new()
        {
            From = EmailRecipient.NoReply,
            SendingModule = string.Empty,
            Subject = $"[Aurora College] Parent Contact Change Report - {DateTime.Today.ToLongDateString()}",
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        List<Attachment> attachments = new()
        {
            new Attachment(report, "Change Report.xlsx", FileContentTypes.ExcelModernFile)
        };

        message.AddRecipient(EmailRecipient.InfoTechTeam, EmailRecipientType.To);
        message.AddRecipient(EmailRecipient.AbsencesMailbox, EmailRecipientType.To);

        await _emailSender.Send(message, attachments, cancellationToken: cancellationToken);
    }

    public async Task SendAdminLowCreditAlert(double credit)
    {
        string viewModel = $"<p>The SMS Global account has a low balance of ${credit:c}.</p><p>Please top up the account immediately!</p>";

        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", viewModel);

        EmailMessage message = new()
        {
            From = EmailRecipient.NoReply,
            SendingModule = string.Empty,
            Subject = "[Aurora College] SMS Gateway Low Balance Alert",
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        message.AddRecipient(EmailRecipient.InfoTechTeam, EmailRecipientType.To);
        message.AddRecipient(EmailRecipient.AbsencesMailbox, EmailRecipientType.To);

        await _emailSender.Send(message);
    }

    public async Task SendMasterFileConsistencyReportEmail(
        MemoryStream report,
        string emailAddress,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>MasterFile Consistency Report generated {DateTime.Today.ToLongDateString()} is attached.</p>";

        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", viewModel);

        EmailMessage message = new()
        {
            From = EmailRecipient.NoReply,
            SendingModule = string.Empty,
            Subject = $"[Aurora College] MasterFile Consistency Report - {DateTime.Today.ToLongDateString()}",
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        List<Attachment> attachments = new()
        {
            new Attachment(report, "Consistency Report.xlsx", FileContentTypes.ExcelModernFile)
        };
        
        Result<EmailRecipient> recipient = EmailRecipient.Create(emailAddress, emailAddress);

        if (recipient.IsSuccess)
            message.AddRecipient(recipient.Value, EmailRecipientType.To);
        else
            return;

        await _emailSender.Send(message, attachments, cancellationToken: cancellationToken);
    }

    public async Task SendServiceLogEmail(ServiceLogEmail notification)
    {
        string viewModel = $"The following messages were logged by {notification.Source} when it ran today.<br>";
        foreach (string line in notification.Log)
            viewModel += line + "<br>";

        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/PlainEmail.cshtml", viewModel);

        EmailMessage message = new()
        {
            From = EmailRecipient.NoReply,
            SendingModule = string.Empty,
            Subject = $"[Aurora College] Service Log Output - {notification.Source}",
            BodyText = rendered.PlainText,
            BodyHtml = rendered.Html,
            CreatedAt = DateTimeOffset.UtcNow
        };

        message.AddRecipients(notification.Recipients, EmailRecipientType.To);
        message.AddRecipient(EmailRecipient.InfoTechTeam, EmailRecipientType.To);

        await _emailSender.Send(message);
    }
}
