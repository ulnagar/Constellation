namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.Absences;
using Core.ValueObjects;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

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
        ParentAttendanceReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StudentName = studentName,
            StartDate = startDate,
            EndDate = endDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAttendanceReportEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, null, null, null, viewModel.Title, body, attachments, cancellationToken);

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
        SchoolAttendanceReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"ATTN: Attendance Coordinator RE: [Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StartDate = startDate,
            EndDate = endDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/SchoolAttendanceReportEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, null, null, null, viewModel.Title, body, attachments, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        return message;
    }
}
