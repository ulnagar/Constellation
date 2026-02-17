namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Domains.AppSettings.Models;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.Lessons;
using Core.ValueObjects;
using MimeKit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendLessonMissedEmail(LessonMissedNotificationEmail notification)
    {
        switch (notification.NotificationType)
        {
            case LessonMissedNotificationEmail.NotificationSequence.First:
                await SendFirstLessonWarningEmail(notification);
                break;
            case LessonMissedNotificationEmail.NotificationSequence.Second:
                await SendSecondLessonWarningEmail(notification);
                break;
            case LessonMissedNotificationEmail.NotificationSequence.Third:
                await SendThirdLessonWarningEmail(notification);
                break;
            case LessonMissedNotificationEmail.NotificationSequence.Final:
                await SendFinalLessonWarningEmail(notification);
                break;
            case LessonMissedNotificationEmail.NotificationSequence.Alert:
                await SendLessonAlertEmail(notification);
                break;
        }
    }

    private async Task SendFirstLessonWarningEmail(LessonMissedNotificationEmail notification)
    {
        LessonsConfiguration? configuration = await _appSettings.Lessons();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendFirstLessonWarningEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(LessonsConfiguration)), true)
                .Warning("Failed to send lesson notification");

            return;
        }

        FirstWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.CoordinatorName,
            SenderTitle = configuration.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FirstWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, configuration.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendSecondLessonWarningEmail(LessonMissedNotificationEmail notification)
    {
        LessonsConfiguration? configuration = await _appSettings.Lessons();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendSecondLessonWarningEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(LessonsConfiguration)), true)
                .Warning("Failed to send lesson notification");

            return;
        }

        SecondWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.CoordinatorName,
            SenderTitle = configuration.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, configuration.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendThirdLessonWarningEmail(LessonMissedNotificationEmail notification)
    {
        LessonsConfiguration? configuration = await _appSettings.Lessons();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendThirdLessonWarningEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(LessonsConfiguration)), true)
                .Warning("Failed to send lesson notification");

            return;
        }

        SecondWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.CoordinatorName,
            SenderTitle = configuration.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, configuration.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendFinalLessonWarningEmail(LessonMissedNotificationEmail notification)
    {
        LessonsConfiguration? configuration = await _appSettings.Lessons();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendFinalLessonWarningEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(LessonsConfiguration)), true)
                .Warning("Failed to send lesson notification");

            return;
        }

        FinalWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.CoordinatorName,
            SenderTitle = configuration.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FinalWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, configuration.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendLessonAlertEmail(LessonMissedNotificationEmail notification)
    {
        LessonsConfiguration? configuration = await _appSettings.Lessons();

        if (configuration is null)
        {
            _logger
                .ForContext("Action", nameof(SendLessonAlertEmail))
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(LessonsConfiguration)), true)
                .Warning("Failed to send lesson notification");

            return;
        }

        CoordinatorNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = configuration.CoordinatorName,
            SenderTitle = configuration.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/CoordinatorNotificationEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, configuration.CoordinatorEmail, viewModel.Title, body);
    }

    public async Task SendStudentLessonCompletedEmail(
        Student student,
        string lessonName,
        string courseName,
        CancellationToken cancellationToken)
    {
        //TODO: Change Sender and Title to be configured in settings.
        // 2026-02-17 - R1.18.4

        StudentMarkedPresentEmailViewModel viewModel = new()
        {
            Title = $"Congratulations on finishing your Science Prac!",
            SenderName = "Silvia Rudmann",
            SenderTitle = "R/Head Teacher Science and Agriculture",
            Preheader = "",
            StudentName = student.Name.DisplayName,
            LessonTitle = lessonName,
            Subject = courseName
        };

        List<EmailRecipient> toRecipients = new();
        Result<EmailRecipient> recipient = EmailRecipient.Create(student.Name.DisplayName, student.EmailAddress.Email);
        toRecipients.Add(recipient.Value);

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/StudentMarkedPresentEmail.cshtml", viewModel);

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply.Email, viewModel.Title, body, MessagePriority.Normal, cancellationToken);
    }
}
