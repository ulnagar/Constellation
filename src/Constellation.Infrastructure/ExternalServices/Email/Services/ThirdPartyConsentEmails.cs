namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Core.Models.ThirdPartyConsent;
using Core.ValueObjects;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Templates.Views.Emails.ThirdParty;

public sealed partial class Service : IEmailService
{
    public async Task SendConsentTransactionReceiptToParent(
        List<EmailRecipient> recipients,
        string studentName,
        DateOnly submittedOn,
        Attachment attachment,
        CancellationToken cancellationToken = default)
    {
        TransactionReceiptParentEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Third-party consent receipt - {studentName} {submittedOn:dd-MM-yyyy}",
            StudentName = studentName,
            SubmittedOn = submittedOn
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Third Party Consent",
            viewModel.Title,
            recipients,
            attachments: [ attachment ],
            cancellationToken: cancellationToken);
    }

    public async Task SendConsentRefusedNotification(
        List<EmailRecipient> recipients,
        string studentName,
        DateOnly submittedOn,
        List<Transaction.ConsentResponse> responses,
        CancellationToken cancellationToken = default)
    {
        ConsentRefusedNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Student = studentName,
            SubmittedOn = submittedOn,
            RefusedConsents = responses.Select(entry => entry.ApplicationName).ToList(),
            Title = $"[Aurora College] Third-party consent refused - {studentName} {submittedOn:dd-MM-yyyy}"
        };

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Third Party Consent",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }
}
