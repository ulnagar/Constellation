using Microsoft.AspNetCore.Identity.UI.Services;

namespace Constellation.Portal.Parents.Server.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Sending email to {email} with subject {subject} and body {htmlMessage}.", email, subject, htmlMessage);

        return Task.CompletedTask;
    }
}
