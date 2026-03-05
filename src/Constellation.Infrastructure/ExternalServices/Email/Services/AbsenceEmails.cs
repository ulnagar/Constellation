namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.AppSettings.Models;
using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Application.Domains.Attendance.Absences.Commands.ConvertResponseToAbsenceExplanation;
using Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Core.Errors;
using Core.Models;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Templates.Views.Emails.Absences;

public sealed partial class Service : IEmailService
{
    public async Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendAbsenceReasonToSchoolAdmin))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return;
        }

        AbsenceExplanationToSchoolAdminEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
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

        await _emailSender.Send(toRecipients, EmailRecipient.AbsencesMailbox, $"Absence Explanation Received - {viewModel.StudentName}", body);
    }

    public async Task SendNonResidentialParentAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendNonResidentialParentAbsenceReasonToSchoolAdmin))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return;
        }

        NonResidentialParentAbsenceExplanationToSchoolAdminEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
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

        await _emailSender.Send(toRecipients, EmailRecipient.AbsencesMailbox, $"Non-Residential Parent Absence Explanation Received - {viewModel.StudentName}", body);
    }

    public async Task<Result<EmailDtos.SentEmail>> SendParentWholeAbsenceAlert(
       string familyName,
       List<AbsenceEntry> absences,
       Student student,
       List<EmailRecipient> emailAddresses,
       CancellationToken cancellationToken = default)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendParentWholeAbsenceAlert))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        ParentAbsenceNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "[Aurora College] Absentee Notice - Compulsory School Attendance",
            ParentName = familyName,
            StudentFirstName = student.Name.PreferredName,
            Absences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceNotificationEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(emailAddresses, EmailRecipient.AbsencesMailbox, viewModel.Title, body, MessagePriority.Normal, cancellationToken);

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
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendParentAbsenceDigest))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        ParentAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "[Aurora College] Absentee Notice - Compulsory School Attendance",
            StudentFirstName = student.Name.PreferredName,
            WholeAbsences = wholeAbsences,
            PartialAbsences = partialAbsences,
            ParentName = familyName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceDigestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(emailAddresses, EmailRecipient.AbsencesMailbox, viewModel.Title, body, MessagePriority.Normal, cancellationToken);

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
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendStudentPartialAbsenceExplanationRequest))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        StudentAbsenceExplanationRequestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.Name.DisplayName,
            Link = "https://acos.aurora.nsw.edu.au/",
            Absences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceExplanationRequestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, EmailRecipient.AbsencesMailbox, viewModel.Title, body, MessagePriority.Normal, cancellationToken);

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
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendCoordinatorPartialAbsenceVerificationRequest))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        CoordinatorAbsenceVerificationRequestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "Partial Absence Verification Request",
            StudentName = student.Name.DisplayName,
            SchoolName = enrolment?.SchoolName ?? "your school",
            ClassList = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceVerificationRequestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, EmailRecipient.AbsencesMailbox, viewModel.Title, body, MessagePriority.Normal, cancellationToken);

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
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendCoordinatorAbsenceDigest))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        if (recipients.Count == 0)
            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.UnknownError);

        CoordinatorAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "Absence Explanation Request",
            StudentName = student.Name,
            SchoolName = school.Name,
            WholeAbsences = wholeAbsences,
            PartialAbsences = partialAbsences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceDigestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, EmailRecipient.AbsencesMailbox, viewModel.Title, body, MessagePriority.Normal, cancellationToken);

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
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendStudentAbsenceDigest))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        if (recipients.Count == 0)
            return Result.Failure<EmailDtos.SentEmail>(ApplicationErrors.UnknownError);

        StudentAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.Name,
            StudentId = student.Id,
            PartialAbsences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceDigestEmail.cshtml", viewModel);

        Result<MimeMessage> message = await _emailSender.Send(recipients, EmailRecipient.AbsencesMailbox, viewModel.Title, body, MessagePriority.Normal, cancellationToken);

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
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendNonResidentialParentAbsenceReasonToSchoolAdmin))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return;
        }

        MissedWorkEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "[Aurora College] Missed Classwork Notification",
            StudentName = student.Name.DisplayName,
            Subject = subjectName,
            ClassName = className,
            AbsenceDate = absenceDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/MissedWorkEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.AbsencesMailbox, viewModel.Title, body, MessagePriority.Normal, cancellationToken);
    }
}
