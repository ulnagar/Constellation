namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.Templates.Views.Emails.Auth;
using Core.ValueObjects;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{

    public async Task SendMagicLinkLoginEmail(MagicLinkEmail notification)
    {
        MagicLinkLoginEmailViewModel viewModel = new()
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = "[Aurora College] Portal Login Link",
            ToName = notification.Name,
            Link = notification.Link
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Auth/MagicLinkLoginEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, EmailRecipient.NoReply, viewModel.Title, body);
    }
}
