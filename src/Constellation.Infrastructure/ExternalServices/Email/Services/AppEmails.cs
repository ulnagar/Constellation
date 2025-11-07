namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Services;
using Core.Shared;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendAdminAbsenceSentralAlert(string studentName)
    {
        string viewModel = $"<p>{studentName} cannot be located in the Sentral Users list and does not currently have a Sentral Student Id specified.</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam];

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, "[Aurora College] Student absence notification", body);
    }

    public async Task SendAdminAbsenceContactAlert(string studentName)
    {
        string viewModel = $"<p>Parent contact details for {studentName} cannot be located in Sentral.";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam];

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, "[Aurora College] Constellation Data Issue Identified", body);
    }

    public async Task SendParentContactChangeReportEmail(
        MemoryStream report,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>Parent Contact Change Report for {DateTime.Today.ToLongDateString()} is attached.</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam, EmailRecipient.AuroraCollege];

        List<Attachment> attachments = new()
        {
            new Attachment(report, "Change Report.xlsx", FileContentTypes.ExcelModernFile)
        };

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply.Email, $"[Aurora College] Parent Contact Change Report - {DateTime.Today.ToLongDateString()}", body, attachments, cancellationToken);
    }

    public async Task SendAdminLowCreditAlert(double credit)
    {
        string viewModel = $"<p>The SMS Global account has a low balance of ${credit:c}.</p><p>Please top up the account immediately!</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam, EmailRecipient.AuroraCollege];

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply.Email, "[Aurora College] SMS Gateway Low Balance Alert", body);
    }

    public async Task SendMasterFileConsistencyReportEmail(
        MemoryStream report,
        string emailAddress,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>MasterFile Consistency Report generated {DateTime.Today.ToLongDateString()} is attached.</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = new();

        Result<EmailRecipient> recipient = EmailRecipient.Create(emailAddress, emailAddress);

        if (recipient.IsSuccess)
            toRecipients.Add(recipient.Value);

        List<Attachment> attachments = new()
        {
            new Attachment(report, "Consistency Report.xlsx", FileContentTypes.ExcelModernFile)
        };

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, $"[Aurora College] MasterFile Consistency Report - {DateTime.Today.ToLongDateString()}", body, attachments, cancellationToken);
    }

    public async Task SendServiceLogEmail(ServiceLogEmail notification)
    {
        string viewModel = $"The following messages were logged by {notification.Source} when it ran today.<br>";
        foreach (string line in notification.Log)
            viewModel += line + "<br>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> recipients = notification.Recipients;
        if (recipients.All(entry => entry.Email != EmailRecipient.InfoTechTeam.Email))
        {
            recipients.Add(EmailRecipient.InfoTechTeam);
        }

        await _emailSender.Send(recipients, EmailRecipient.NoReply, $"[Aurora College] Service Log Output - {notification.Source}", body);
    }
}
