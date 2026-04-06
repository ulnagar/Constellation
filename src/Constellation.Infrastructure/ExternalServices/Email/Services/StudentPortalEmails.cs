namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Core.ValueObjects;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendSupportTicketRequest(
        EmailRecipient submitter,
        string subject,
        string description,
        CancellationToken cancellationToken = default)
    {
        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", description);

        await BuildAndSendEmail(
            body,
            submitter,
            subject,
            [ EmailRecipient.InfoTechTeam, EmailRecipient.SupportQueue ],
            cancellationToken: cancellationToken);
    }
}
