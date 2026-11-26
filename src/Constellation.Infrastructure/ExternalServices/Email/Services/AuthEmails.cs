namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using Constellation.Core.Models.Messaging.Email;
using Constellation.Core.Models.Messaging.Email.Enums;
using Constellation.Infrastructure.Templates.Views.Emails.Auth;
using Core.Models.Messaging.Enums;
using Core.Shared;
using Core.ValueObjects;
using System.ClientModel.Primitives;
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
            MessageModule.Authentication,
            EmailRecipient.NoReply,
            null,
            viewModel.Title,
            rendered.PlainText,
            rendered.Html);

        foreach (EmailRecipient entry in notification.Recipients)
            message.AddRecipient(entry, EmailRecipientType.To);

        await _emailSender.Send(message, includeTracking: false);
    }

    public async Task<Result> SendEnrolmentPortalLoginEmail(
        Name name,
        string link,
        EmailRecipient recipient)
    {
        EnrolmentPortalEmailLoginEmailViewModel viewModel = new()
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = "[Aurora College] Enrolments Portal Login Link",
            Name = name,
            Link = link
        };

        RenderedEmail rendered = await _razorService.RenderEmail(viewModel.ViewLocation, viewModel);

        EmailMessage message = new(
            MessageModule.Authentication,
            EmailRecipient.NoReply,
            null,
            viewModel.Title,
            rendered.PlainText,
            rendered.Html);

        message.AddRecipient(recipient, EmailRecipientType.To);

        return await _emailSender.Send(message, includeTracking: false);
    }
}
