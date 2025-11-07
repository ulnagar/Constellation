namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.RollMarking;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendDailyRollMarkingReport(
    List<RollMarkingEmailDto> entries,
    DateOnly reportDate,
    Dictionary<string, string> recipients)
    {
        DailyReportEmailViewModel viewModel = new()
        {
            Preheader = "This is an automated email. No action is required outside of school hours.",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}",
            RollEntries = entries
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/DailyReportEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = new();

        foreach (KeyValuePair<string, string> entry in recipients)
        {
            if (toRecipients.Any(recipient => recipient.Email == entry.Value))
            {
                continue;
            }

            Result<EmailRecipient> recipient = EmailRecipient.Create(entry.Key, entry.Value);

            if (recipient.IsSuccess)
                toRecipients.Add(recipient.Value);
        }

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, viewModel.Title, body);
    }

    public async Task SendNoRollMarkingReport(
        DateOnly reportDate,
        Dictionary<string, string> recipients)
    {
        DailyReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}"
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/NoReportEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = new();

        foreach (KeyValuePair<string, string> entry in recipients)
        {
            if (toRecipients.Any(recipient => recipient.Email == entry.Value))
            {
                continue;
            }

            Result<EmailRecipient> recipient = EmailRecipient.Create(entry.Key, entry.Value);

            if (recipient.IsSuccess)
                toRecipients.Add(recipient.Value);
        }

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, viewModel.Title, body);
    }
}
