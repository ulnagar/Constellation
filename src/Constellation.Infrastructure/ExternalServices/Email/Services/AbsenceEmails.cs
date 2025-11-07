namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Domains.Attendance.Absences.Commands.ConvertResponseToAbsenceExplanation;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.Absences;
using Core.Models;
using Core.ValueObjects;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        AbsenceExplanationToSchoolAdminEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Received",
            StudentName = notificationEmail.StudentName
        };

        foreach (EmailDtos.AbsenceResponseEmail.AbsenceDto absence in notificationEmail.WholeAbsences)
        {
            viewModel.Absences.Add(new AbsenceExplanationToSchoolAdminEmailViewModel.AbsenceDto
            {
                AbsenceDate = absence.AbsenceDate,
                PeriodName = absence.PeriodName,
                ClassName = absence.ClassName,
                Explanation = absence.Explanation,
                Source = absence.ReportedBy,
                Type = absence.AbsenceType.Value,
                AbsenceTime = absence.AbsenceTimeframe
            });
        }

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/AbsenceExplanationToSchoolAdminEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = new();
        foreach (string entry in notificationEmail.Recipients)
        {
            if (toRecipients.Any(recipient => recipient.Email == entry))
            {
                continue;
            }

            Result<EmailRecipient> recipient = EmailRecipient.Create(entry, entry);

            if (recipient.IsSuccess)
                toRecipients.Add(recipient.Value);
        }

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, $"Absence Explanation Received - {viewModel.StudentName}", body);
    }

    public async Task SendNonResidentialParentAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        NonResidentialParentAbsenceExplanationToSchoolAdminEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Received",
            StudentName = notificationEmail.StudentName
        };

        foreach (EmailDtos.AbsenceResponseEmail.AbsenceDto absence in notificationEmail.WholeAbsences)
        {
            viewModel.Absences.Add(new()
            {
                AbsenceDate = absence.AbsenceDate,
                PeriodName = absence.PeriodName,
                ClassName = absence.ClassName,
                Explanation = absence.Explanation,
                Source = absence.ReportedBy,
                Type = absence.AbsenceType.Value,
                AbsenceTime = absence.AbsenceTimeframe
            });
        }

        string body = await _razorService.RenderViewToStringAsync(NonResidentialParentAbsenceExplanationToSchoolAdminEmailViewModel.ViewLocation, viewModel);

        List<EmailRecipient> toRecipients = new();
        foreach (string entry in notificationEmail.Recipients)
        {
            if (toRecipients.Any(recipient => recipient.Email == entry))
            {
                continue;
            }

            Result<EmailRecipient> recipient = EmailRecipient.Create(entry, entry);

            if (recipient.IsSuccess)
                toRecipients.Add(recipient.Value);
        }

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, $"Non-Residential Parent Absence Explanation Received - {viewModel.StudentName}", body);
    }

    public async Task<Result<EmailDtos.SentEmail>> SendParentWholeAbsenceAlert(
       string familyName,
       List<AbsenceEntry> absences,
       Student student,
       List<EmailRecipient> emailAddresses,
       CancellationToken cancellationToken = default)
    {
        ParentAbsenceNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
            ParentName = familyName,
            StudentFirstName = student.Name.PreferredName,
            Absences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceNotificationEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(emailAddresses, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message.IsFailure)
        {
            return Result.Failure<EmailDtos.SentEmail>(message.Error);
        }

        return new EmailDtos.SentEmail
        {
            message = body,
            id = message.Value.MessageId,
            recipients = message.Value.To.ToString()
        };
    }

    public async Task<Result<EmailDtos.SentEmail>> SendParentAbsenceDigest(
        string familyName,
        List<AbsenceEntry> wholeAbsences,
        List<AbsenceEntry> partialAbsences,
        Student student,
        List<EmailRecipient> emailAddresses,
        CancellationToken cancellationToken = default)
    {
        ParentAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
            StudentFirstName = student.Name.PreferredName,
            WholeAbsences = wholeAbsences,
            PartialAbsences = partialAbsences,
            ParentName = familyName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceDigestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(emailAddresses, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message.IsFailure)
            return Result.Failure<EmailDtos.SentEmail>(message.Error);

        return new EmailDtos.SentEmail()
        {
            message = body,
            id = message.Value.MessageId,
            recipients = message.Value.To.ToString()
        };
    }

    public async Task<Result<EmailDtos.SentEmail>> SendStudentPartialAbsenceExplanationRequest(
        List<AbsenceEntry> absences,
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        StudentAbsenceExplanationRequestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.Name.DisplayName,
            Link = $"https://acos.aurora.nsw.edu.au/",
            Absences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceExplanationRequestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message.IsFailure)
            return Result.Failure<EmailDtos.SentEmail>(message.Error);

        return new EmailDtos.SentEmail()
        {
            message = body,
            id = message.Value.MessageId,
            recipients = message.Value.To.ToString()
        };
    }

    public async Task<Result<EmailDtos.SentEmail>> SendCoordinatorPartialAbsenceVerificationRequest(
        List<AbsenceExplanation> absences,
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        CoordinatorAbsenceVerificationRequestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Partial Absence Verification Request",
            StudentName = student.Name.DisplayName,
            SchoolName = enrolment?.SchoolName ?? "your school",
            ClassList = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceVerificationRequestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message.IsFailure)
            return Result.Failure<EmailDtos.SentEmail>(message.Error);

        return new EmailDtos.SentEmail()
        {
            message = body,
            id = message.Value.MessageId,
            recipients = message.Value.To.ToString()
        };
    }

    public async Task<Result<EmailDtos.SentEmail>> SendCoordinatorAbsenceDigest(
        List<AbsenceEntry> wholeAbsences,
        List<AbsenceEntry> partialAbsences,
        Student student,
        School school,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        if (recipients is null || recipients.Count == 0)
            return null;

        CoordinatorAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Request",
            StudentName = student.Name,
            SchoolName = school.Name,
            WholeAbsences = wholeAbsences,
            PartialAbsences = partialAbsences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceDigestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message.IsFailure)
            return Result.Failure<EmailDtos.SentEmail>(message.Error);

        return new EmailDtos.SentEmail()
        {
            message = body,
            id = message.Value.MessageId,
            recipients = message.Value.To.ToString()
        };
    }

    public async Task<Result<EmailDtos.SentEmail>> SendStudentAbsenceDigest(
        List<AbsenceEntry> absences,
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        if (recipients is null || recipients.Count == 0)
            return null;

        StudentAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.Name,
            StudentId = student.Id,
            PartialAbsences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceDigestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message.IsFailure)
            return Result.Failure<EmailDtos.SentEmail>(message.Error);

        return new EmailDtos.SentEmail()
        {
            message = body,
            id = message.Value.MessageId,
            recipients = message.Value.To.ToString()
        };
    }

    public async Task SendMissedWorkEmail(
        Student student,
        string subjectName,
        string className,
        DateOnly absenceDate,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        MissedWorkEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "[Aurora College] Missed Classwork Notification",
            StudentName = student.Name.DisplayName,
            Subject = subjectName,
            ClassName = className,
            AbsenceDate = absenceDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/MissedWorkEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);
    }
}
