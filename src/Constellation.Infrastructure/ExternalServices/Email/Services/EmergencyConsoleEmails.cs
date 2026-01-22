namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Core.ValueObjects;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using Templates.Views.Emails.Emergency;

public sealed partial class Service : IEmailService
{
    public async Task<Result<string>> SendEmergencyConsoleEmail(
        AlertRecipient recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
        EmergencyConsoleEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = "[Aurora College] Emergency Alert",
            Message = message
        };

        string body = await _razorService.RenderViewToStringAsync(EmergencyConsoleEmailViewModel.ViewLocation, viewModel);

        Result<EmailRecipient> emailRecipient = recipient.GetEmailRecipient();

        if (emailRecipient.IsFailure)
            return Result.Failure<string>(emailRecipient.Error);

        Result<MimeMessage> email = await _emailSender.Send([ emailRecipient.Value ], EmailRecipient.NoReply, viewModel.Title, body, MessagePriority.Urgent, cancellationToken);

        if (email.IsSuccess)
            return Result.Success(email.Value.MessageId);

        return Result.Failure<string>(email.Error);
    }
}