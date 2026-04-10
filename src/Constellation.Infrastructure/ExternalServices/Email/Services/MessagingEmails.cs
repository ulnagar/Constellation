namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Constellation.Infrastructure.Templates.Views.Emails.Messaging;
using Core.Models.Messaging.Drafts;
using Core.ValueObjects;

public sealed partial class Service : IEmailService
{
    public async Task<Result> SendQueuedMessage(
        MessageSender sender,
        EmailRecipient receiver,
        string subject,
        string messageBody,
        CancellationToken cancellationToken = default)
    {
        EmergencyConsoleEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = sender.Name,
            SenderTitle = string.Empty,
            Title = subject,
            Message = messageBody
        };

        return await BuildAndSendEmail(
            viewModel,
            sender,
            "Messaging",
            subject,
            [receiver],
            cancellationToken: cancellationToken);
    }

    public async Task<Result> SendQueuedMessageLog(
        EmailRecipient receiver,
        QueuedMessage message,
        CancellationToken cancellationToken = default)
    {
        QueuedMessageLogEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"Delivery Report: {message.Subject}",
            Message = message
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "Messaging",
            viewModel.Title,
            [receiver],
            cancellationToken: cancellationToken);
    }
}