namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Templates.Views.Emails.Contacts;

public sealed partial class Service : IEmailService
{
    public async Task SendWelcomeEmailToCoordinator(
        List<EmailRecipient> recipients,
        string schoolName,
        CancellationToken cancellationToken = default)
    {
        NewACCoordinatorEmailViewModel viewModel = new()
        {
            Title = $"Welcome to Aurora College!",
            SenderName = "Virginia Cluff",
            SenderTitle = "Instructional Leader",
            Preheader = "",
            PartnerSchool = schoolName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewACCoordinatorEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendWelcomeEmailToSciencePracTeacher(
        List<EmailRecipient> recipients,
        string schoolName,
        CancellationToken cancellationToken = default)
    {
        NewSciencePracTeacherEmailViewModel viewModel = new()
        {
            Title = $"Welcome to Aurora College!",
            SenderName = "Fiona Boneham",
            SenderTitle = "Science Practical Coordinator",
            Preheader = "",
            PartnerSchool = schoolName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewSciencePracTeacherEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }
}
