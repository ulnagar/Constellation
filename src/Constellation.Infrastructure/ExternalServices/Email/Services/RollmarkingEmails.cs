namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.AppSettings.Models;
using Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Core.Errors;
using Core.Shared;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Templates.Views.Emails.RollMarking;

public sealed partial class Service : IEmailService
{
    public async Task SendDailyRollMarkingReport(
    List<RollMarkingEmailDto> entries,
    DateOnly reportDate,
    List<EmailRecipient> recipients)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendDailyRollMarkingReport))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return;
        }

        DailyReportEmailViewModel viewModel = new()
        {
            Preheader = "This is an automated email. No action is required outside of school hours.",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}",
            RollEntries = entries
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/DailyReportEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body);
    }

    public async Task SendNoRollMarkingReport(
        DateOnly reportDate,
        List<EmailRecipient> recipients)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendNoRollMarkingReport))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return;
        }

        DailyReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}"
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/NoReportEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body);
    }
}
