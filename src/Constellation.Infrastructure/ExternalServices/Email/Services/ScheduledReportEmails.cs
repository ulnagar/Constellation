namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Core.ValueObjects;
using System.Net.Mail;
using System.Threading.Tasks;
using Templates.Views.Emails.ScheduledReports;

public sealed partial class Service : IEmailService
{
    public async Task ForwardCompletedScheduledReport(
        EmailRecipient recipient,
        Attachment attachment,
        CancellationToken cancellationToken = default)
    {
        CompletedScheduledReportViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Scheduled Report",
            Recipient = recipient.Name
        };

        string body = await _razorService.RenderViewToStringAsync(CompletedScheduledReportViewModel.ViewLocation, viewModel);

        await _emailSender.Send([recipient], EmailRecipient.NoReply, viewModel.Title, body, [attachment], cancellationToken);
    }
}
