namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Services;
using Core.ValueObjects;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendAdminAbsenceSentralAlert(string studentName)
    {
        string viewModel = $"<p>{studentName} cannot be located in the Sentral Users list and does not currently have a Sentral Student Id specified.</p>";

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "[Aurora College] Student absence notification",
            [EmailRecipient.InfoTechTeam]);
    }

    public async Task SendAdminAbsenceContactAlert(string studentName)
    {
        string viewModel = $"<p>Parent contact details for {studentName} cannot be located in Sentral.";

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "[Aurora College] Constellation Data Issue Identified",
            [EmailRecipient.InfoTechTeam]);
    }

    public async Task SendParentContactChangeReportEmail(
        MemoryStream report,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>Parent Contact Change Report for {DateTime.Today.ToLongDateString()} is attached.</p>";

        using Attachment attachment = new(report, "Change Report.xlsx", FileContentTypes.ExcelModernFile);

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            $"[Aurora College] Parent Contact Change Report - {DateTime.Today.ToLongDateString()}",
            [EmailRecipient.InfoTechTeam, EmailRecipient.AbsencesMailbox, EmailRecipient.AuroraCollege],
            attachments: [ attachment ],
            cancellationToken: cancellationToken);
    }

    public async Task SendAdminLowCreditAlert(double credit)
    {
        string viewModel = $"<p>The SMS Global account has a low balance of ${credit:c}.</p><p>Please top up the account immediately!</p>";

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "[Aurora College] SMS Gateway Low Balance Alert",
            [EmailRecipient.InfoTechTeam, EmailRecipient.AbsencesMailbox, EmailRecipient.AuroraCollege]);
    }

    public async Task SendMasterFileConsistencyReportEmail(
        MemoryStream report,
        string emailAddress,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>MasterFile Consistency Report generated {DateTime.Today.ToLongDateString()} is attached.</p>";

        using Attachment attachment = new(report, "Consistency Report.xlsx", FileContentTypes.ExcelModernFile);

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            $"[Aurora College] MasterFile Consistency Report - {DateTime.Today.ToLongDateString()}",
            [ emailAddress ],
            attachments: [ attachment ],
            cancellationToken: cancellationToken);
    }

    public async Task SendServiceLogEmail(ServiceLogEmail notification)
    {
        string viewModel = $"The following messages were logged by {notification.Source} when it ran today.<br>";
        foreach (string line in notification.Log)
            viewModel += line + "<br>";

        notification.Recipients.Add(EmailRecipient.InfoTechTeam);

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            $"[Aurora College] Service Log Output - {notification.Source}",
            notification.Recipients);
    }
}
