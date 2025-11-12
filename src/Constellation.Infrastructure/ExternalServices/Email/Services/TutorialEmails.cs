namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Interfaces.Services;
using Constellation.Core.Shared;
using Core.Models.LinkedSystems;
using Core.Models.Tutorials;
using Core.ValueObjects;
using MimeKit;
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

        string body = await _razorService.RenderViewToStringAsync(TutorialRequestReceivedEmailViewModel.ViewLocation, viewModel);

        Result<MimeMessage> emailSendOperation = await _emailSender.Send(recipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure(emailSendOperation.Error);

        return Result.Success();
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

        string body = await _razorService.RenderViewToStringAsync(TutorialRequestReceivedNotificationEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure(emailSendOperation.Error);

        return Result.Success();
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

        string body = await _razorService.RenderViewToStringAsync(TutorialRequestApprovedNotificationEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure(emailSendOperation.Error);

        return Result.Success();
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
            Subject = tutorialRequest.Subject
        };

        string body = await _razorService.RenderViewToStringAsync(TutorialRequestRejectedEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure(emailSendOperation.Error);

        return Result.Success();
    }

    public async Task<Result> SendTutorialRequestScheduledEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        Team tutorialTeam,
        List<(string Period, string Teacher)> periods,
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
            StartDate = tutorialRequest.Plan.StartDate,
            TutorialTeam = tutorialTeam,
            ScheduledPeriods = periods
        };

        string body = await _razorService.RenderViewToStringAsync(TutorialRequestScheduledEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure<string>(emailSendOperation.Error);

        return Result.Success();
    }

}
