namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Core.Shared;
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

        Result result = await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "RollMarking",
            viewModel.Title,
            [ recipient ],
            attachments: [ attachment ],
            cancellationToken: cancellationToken);

        if (result.IsFailure)
        {
            GetLogger()
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send roll marking report email");
        }
    }
}
