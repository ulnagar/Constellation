namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.AppSettings.Models;
using Constellation.Application.Interfaces.Services;
using Core.Errors;
using Core.Shared;
using Core.ValueObjects;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Templates.Views.Emails.Absences;

public sealed partial class Service : IEmailService
{
    public async Task<Result> SendParentAttendanceReportEmail(
    string studentName,
    DateOnly startDate,
    DateOnly endDate,
    List<EmailRecipient> recipients,
    List<Attachment> attachments,
    CancellationToken cancellationToken = default)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendParentAttendanceReportEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        ParentAttendanceReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = $"[Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StudentName = studentName,
            StartDate = startDate,
            EndDate = endDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAttendanceReportEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, [], [], string.Empty , viewModel.Title, body, attachments, MessagePriority.Normal, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        return message;
    }

    public async Task<Result> SendSchoolAttendanceReportEmail(
        DateOnly startDate,
        DateOnly endDate,
        List<EmailRecipient> recipients,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendSchoolAttendanceReportEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        SchoolAttendanceReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = $"ATTN: Attendance Coordinator RE: [Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StartDate = startDate,
            EndDate = endDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/SchoolAttendanceReportEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, [], [], string.Empty, viewModel.Title, body, attachments, MessagePriority.Normal, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        return message;
    }
}
