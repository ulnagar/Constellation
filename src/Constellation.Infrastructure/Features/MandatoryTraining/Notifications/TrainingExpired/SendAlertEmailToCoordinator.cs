namespace Constellation.Infrastructure.Features.MandatoryTraining.Notifications.TrainingExpired;

using Constellation.Application.Features.MandatoryTraining.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SendAlertEmailToCoordinator : INotificationHandler<TrainingExpiredNotification>
{
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IEmailGateway _emailSender;

    public SendAlertEmailToCoordinator(IRazorViewToStringRenderer razorService, IEmailGateway emailSender)
    {
        _razorService = razorService;
        _emailSender = emailSender;
    }
    public async Task Handle(TrainingExpiredNotification notification, CancellationToken cancellationToken)
    {
        var viewModel = new TrainingExpiredAlertViewModel
        {
            Preheader = "",
            SenderName = "",
            SenderTitle = "",
            Title = "[Aurora College] Mandatory Training Expiry"
        };

        foreach (var entry in notification.ExpiredCertificates)
        {
            viewModel.Courses.Add(entry.Key, entry.Value.ToShortDateString());
        }

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiredAlertEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();

        if (!toRecipients.Any(recipient => recipient.Value == notification.Teacher.Email))
            toRecipients.Add(notification.Teacher.Name, notification.Teacher.Email);

        if (!toRecipients.Any(recipient => recipient.Value == notification.HeadTeacher.Email))
            toRecipients.Add(notification.HeadTeacher.Name, notification.HeadTeacher.Email);

        if (!toRecipients.Any(recipient => recipient.Value == notification.Coordinator.Email))
            toRecipients.Add(notification.Coordinator.Name, notification.Coordinator.Email);

        await _emailSender.Send(toRecipients, null, viewModel.Title, body);
    }
}
