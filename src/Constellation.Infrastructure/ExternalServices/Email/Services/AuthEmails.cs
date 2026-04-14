namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using Constellation.Core.Models.Messaging.Email;
using Constellation.Core.Models.Messaging.Email.Enums;
using Constellation.Infrastructure.Templates.Views.Emails.Auth;
using Core.ValueObjects;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{

    public async Task SendMagicLinkLoginEmail(MagicLinkEmail notification)
    {
        MagicLinkLoginEmailViewModel viewModel = new(
            notification.Name,
            notification.Link)
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = "[Aurora College] Portal Login Link"
        };

        RenderedEmail rendered = await _razorService.RenderEmail("/Views/Emails/Auth/MagicLinkLoginEmail.cshtml", viewModel);

        EmailMessage message = new(
            "Authentication",
            EmailRecipient.NoReply,
            null,
            viewModel.Title,
            rendered.PlainText,
            rendered.Html);

        foreach (EmailRecipient entry in notification.Recipients)
            message.AddRecipient(entry, EmailRecipientType.To);

        await _emailSender.Send(message, includeTracking: false);
    }
}
