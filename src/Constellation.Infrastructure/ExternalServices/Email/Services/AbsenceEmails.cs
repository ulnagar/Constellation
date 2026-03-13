namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.AppSettings.Models;
using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Application.Domains.Attendance.Absences.Commands.ConvertResponseToAbsenceExplanation;
using Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Core.Errors;
using Core.Models;
using Core.Models.Messaging.Email;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
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
            GetLogger()
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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel, 
            EmailRecipient.AbsencesMailbox, 
            "Absences",
            $"Absence Explanation Received - {viewModel.StudentName}", 
            notificationEmail.Recipients);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }
    }

    public async Task SendNonResidentialParentAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences();

        if (configuration is null)
        {
            GetLogger()
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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            $"Non-Residential Parent Absence Explanation Received - {viewModel.StudentName}",
            notificationEmail.Recipients);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }
    }

    public async Task<Result<EmailMessage>> SendParentWholeAbsenceAlert(
       string familyName,
       List<AbsenceEntry> absences,
       Student student,
       List<EmailRecipient> emailAddresses,
       CancellationToken cancellationToken = default)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailMessage>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            viewModel.Title,
            emailAddresses);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }

        return result;
    }

    public async Task<Result<EmailMessage>> SendParentAbsenceDigest(
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
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailMessage>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            viewModel.Title,
            emailAddresses);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }

        return result;
    }

    public async Task<Result<EmailMessage>> SendStudentPartialAbsenceExplanationRequest(
        List<AbsenceEntry> absences,
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailMessage>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        StudentAbsenceExplanationRequestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.ContactName,
            SenderTitle = configuration.ContactTitle,
            Title = "[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.Name.DisplayName,
            Absences = absences
        };

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            viewModel.Title,
            recipients);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }

        return result;
    }

    public async Task<Result<EmailMessage>> SendCoordinatorPartialAbsenceVerificationRequest(
        List<AbsenceExplanation> absences,
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailMessage>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            viewModel.Title,
            recipients);
        
        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }

        return result;
    }

    public async Task<Result<EmailMessage>> SendCoordinatorAbsenceDigest(
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
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailMessage>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        if (recipients.Count == 0)
            return Result.Failure<EmailMessage>(ApplicationErrors.UnknownError);

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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            viewModel.Title,
            recipients);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }

        return result;
    }

    public async Task<Result<EmailMessage>> SendStudentAbsenceDigest(
        List<AbsenceEntry> absences,
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        AbsencesConfiguration? configuration = await _appSettings.Absences(cancellationToken);

        if (configuration is null)
        {
            GetLogger()
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)), true)
                .Warning("Failed to send absence email");

            return Result.Failure<EmailMessage>(ApplicationErrors.InvalidConfiguration(nameof(AbsencesConfiguration)));
        }

        if (recipients.Count == 0)
            return Result.Failure<EmailMessage>(ApplicationErrors.UnknownError);

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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            viewModel.Title,
            recipients);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }

        return result;
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
            GetLogger()
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

        Result<EmailMessage> result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AbsencesMailbox,
            "Absences",
            viewModel.Title,
            recipients);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send absence email");
        }
    }
}
