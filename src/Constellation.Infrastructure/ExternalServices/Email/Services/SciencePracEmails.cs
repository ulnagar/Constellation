namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Extensions;
using Constellation.Application.Domains.AppSettings.Models;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.Lessons;
using Core.Models.StaffMembers;
using Core.ValueObjects;
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
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        await BuildAndSendEmail(
            viewModel,
            configuration.Recipient,
            "Science Practicals",
            viewModel.Title,
            notification.Recipients);
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
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        await BuildAndSendEmail(
            viewModel,
            configuration.Recipient,
            "Science Practicals",
            viewModel.Title,
            notification.Recipients);
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
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        await BuildAndSendEmail(
            viewModel,
            configuration.Recipient,
            "Science Practicals",
            viewModel.Title,
            notification.Recipients);
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
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        await BuildAndSendEmail(
            viewModel,
            configuration.Recipient,
            "Science Practicals",
            viewModel.Title,
            notification.Recipients);
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

        await BuildAndSendEmail(
            viewModel,
            configuration.Recipient,
            "Science Practicals",
            viewModel.Title,
            notification.Recipients);
    }

    public async Task SendStudentLessonCompletedEmail(
        Student student,
        string lessonName,
        string courseName,
        CancellationToken cancellationToken)
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

        StaffMember headTeacher = configuration.Contacts.First().Key;

        StudentMarkedPresentEmailViewModel viewModel = new()
        {
            Title = $"Congratulations on finishing your Science Prac!",
            SenderName = headTeacher.Name.DisplayName,
            SenderTitle = "R/Head Teacher Science and Agriculture",
            Preheader = "",
            StudentName = student.Name.DisplayName,
            LessonTitle = lessonName,
            Subject = courseName
        };

        Result<EmailRecipient> recipient = student.GetEmailRecipient();

        if (recipient.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to send lesson notification");

            return;
        }

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "Science Practicals",
            viewModel.Title,
            [recipient.Value],
            cancellationToken: cancellationToken);
    }
}
