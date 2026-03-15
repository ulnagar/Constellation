namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Core.ValueObjects;
using Templates.Views.Emails.Emergency;

public sealed partial class Service : IEmailService
{
    public async Task<Result> SendEmergencyConsoleEmail(
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

        Result<EmailRecipient> emailRecipient = recipient.GetEmailRecipient();

        if (emailRecipient.IsFailure)
            return Result.Failure(emailRecipient.Error);

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "Emergency Console",
            viewModel.Title,
            [emailRecipient.Value],
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}