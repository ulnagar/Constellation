namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Interfaces.Services;
using Constellation.Core.Shared;
using Core.Models.Tutorials;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Templates.Views.Emails.Tutorials;

public sealed partial class Service : IEmailService
{
    public async Task<Result> SendTutorialRequestReceivedEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        CancellationToken cancellationToken = default)
    {
        TutorialRequestReceivedEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Tutorial Support request received",
            Student = tutorialRequest.Student,
            Grade = tutorialRequest.Grade,
            School = tutorialRequest.School,
            Justification = tutorialRequest.Justification,
            Type = tutorialRequest.Type,
            Subject = tutorialRequest.Subject
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Tutorial Support",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task<Result> SendTutorialRequestReceivedNotificationEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        CancellationToken cancellationToken = default)
    {
        TutorialRequestReceivedNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Tutorial Support request received",
            Student = tutorialRequest.Student,
            Grade = tutorialRequest.Grade,
            School = tutorialRequest.School,
            Justification = tutorialRequest.Justification,
            Type = tutorialRequest.Type,
            Subject = tutorialRequest.Subject,
            RequestId = tutorialRequest.Id
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Tutorial Support",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task<Result> SendTutorialRequestApprovedNotificationEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        CancellationToken cancellationToken = default)
    {
        TutorialRequestApprovedNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Tutorial Support request scheduling required",
            Student = tutorialRequest.Student,
            Grade = tutorialRequest.Grade,
            School = tutorialRequest.School,
            Type = tutorialRequest.Type,
            Subject = tutorialRequest.Subject,
            RequestId = tutorialRequest.Id
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Tutorial Support",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task<Result> SendTutorialRequestRejectedEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        CancellationToken cancellationToken = default)
    {
        TutorialRequestRejectedEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Tutorial Support request rejected",
            Student = tutorialRequest.Student,
            Grade = tutorialRequest.Grade,
            School = tutorialRequest.School,
            Type = tutorialRequest.Type,
            Subject = tutorialRequest.Subject,
            Reason = tutorialRequest.Notes.OrderBy(entry => entry.SubmittedAt).Last().Message
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Tutorial Support",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

    public async Task<Result> SendTutorialRequestScheduledEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        string teamName,
        List<(string Period, string Teacher)> periods,
        DateOnly startDate,
        CancellationToken cancellationToken = default)
    {
        TutorialRequestScheduledEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Tutorial Support scheduled",
            Student = tutorialRequest.Student,
            Grade = tutorialRequest.Grade,
            School = tutorialRequest.School,
            Type = tutorialRequest.Type,
            Subject = tutorialRequest.Subject,
            StartDate = startDate,
            TutorialTeam = teamName,
            ScheduledPeriods = periods
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Tutorial Support",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }

}
