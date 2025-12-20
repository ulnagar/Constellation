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
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Received",
            Message = message
        };

        string body = await _razorService.RenderViewToStringAsync(EmergencyConsoleEmailViewModel.ViewLocation, viewModel);

        Result<EmailRecipient> emailRecipient = recipient.GetEmailRecipient();

        if (emailRecipient.IsFailure)
            return Result.Failure<string>(emailRecipient.Error);

#if DEBUG
        return Result.Success($"{GenerateSecureRandomString(12)}.{GenerateSecureRandomString(13)}@8912sch000sa005");
#endif

        Result<MimeMessage> email = await _emailSender.Send([ emailRecipient.Value ], EmailRecipient.NoReply, $"Emergency Notice", body, MessagePriority.Urgent, cancellationToken);

        if (email.IsSuccess)
            return Result.Success(email.Value.MessageId);

        return Result.Failure<string>(email.Error);
    }

    private const string Pool = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private string GenerateSecureRandomString(int length)
    {
        var result = new StringBuilder(length);
        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            foreach (byte b in bytes)
            {
                // Use modulo operation to select a character from the pool
                result.Append(Pool[b % Pool.Length]);
            }
        }
        return result.ToString();
    }
}