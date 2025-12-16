namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Core.ValueObjects;
using MimeKit;
using Templates.Views.Emails.Emergency;

public sealed partial class Service : IEmailService
{
    public async Task<Result<string>> SendEmergencyConsoleEmail(
        EmailRecipient recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
        EmergencyConsoleEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Received",
            Message = message
        };

        string body = await _razorService.RenderViewToStringAsync(EmergencyConsoleEmailViewModel.ViewLocation, viewModel);

        Result<MimeMessage> email = await _emailSender.Send([recipient], EmailRecipient.NoReply, $"Emergency Notice", body, MessagePriority.Urgent, cancellationToken);

        if (email.IsSuccess)
            return Result.Success(email.Value.MessageId);

        return Result.Failure<string>(email.Error);
    }
}