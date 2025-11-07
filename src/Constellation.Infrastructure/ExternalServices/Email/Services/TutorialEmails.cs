namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Interfaces.Services;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.AwardNominations;
using Core.Models.Tutorials;
using Core.ValueObjects;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        string body = await _razorService.RenderViewToStringAsync(ParentNotificationEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, ccRecipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure<string>(emailSendOperation.Error);

        return body;
    }

    public async Task<Result> SendTutorialRequestRejectedEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        CancellationToken cancellationToken = default)
    {
        string body = await _razorService.RenderViewToStringAsync(ParentNotificationEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, ccRecipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure<string>(emailSendOperation.Error);

        return body;
    }

    public async Task<Result> SendTutorialRequestScheduledEmail(
        List<EmailRecipient> recipients,
        Request tutorialRequest,
        CancellationToken cancellationToken = default)
    {
        string body = await _razorService.RenderViewToStringAsync(ParentNotificationEmailViewModel.ViewLocation, viewModel);

        var emailSendOperation = await _emailSender.Send(recipients, ccRecipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);

        if (emailSendOperation.IsFailure)
            return Result.Failure<string>(emailSendOperation.Error);

        return body;
    }

}
