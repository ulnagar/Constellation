namespace Constellation.Infrastructure.Features.MandatoryTraining.Notifications.TrainingExpiringSoon;

using Constellation.Application.Features.MandatoryTraining.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SendWarningEmailToStaffMember : INotificationHandler<TrainingExpiringSoonNotification>
{
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly IEmailGateway _emailSender;

    public SendWarningEmailToStaffMember(IRazorViewToStringRenderer razorService, IEmailGateway emailSender)
    {
        _razorService = razorService;
        _emailSender = emailSender;
    }

    public async Task Handle(TrainingExpiringSoonNotification notification, CancellationToken cancellationToken)
    {
        var alertDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14));
        var warningDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        var alertEntries = notification.ExpiringCertificates.Where(entry => entry.Value < alertDate).ToList();
        var warningEntries = notification.ExpiringCertificates.Where(entry => entry.Value > alertDate && entry.Value < warningDate).ToList();

        if (alertEntries.Any())
        {
            var alertViewModel = new TrainingExpiringSoonAlertViewModel
            {
                Preheader = "",
                SenderName = "",
                SenderTitle = "",
                Title = "[Aurora College] Mandatory Training Expiry"
            };

            foreach (var entry in warningEntries)
            {
                alertViewModel.Courses.Add(entry.Key, entry.Value.ToShortDateString());
            }

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiringSoonAlertEmail.cshtml", alertViewModel);

            var toRecipients = new Dictionary<string, string>();
            
            if (!toRecipients.Any(recipient => recipient.Value == notification.Teacher.Email))
                toRecipients.Add(notification.Teacher.Name, notification.Teacher.Email);

            if (!toRecipients.Any(recipient => recipient.Value == notification.HeadTeacher.Email))
                toRecipients.Add(notification.HeadTeacher.Name, notification.HeadTeacher.Email);

            await _emailSender.Send(toRecipients, null, alertViewModel.Title, body);
        }

        if (warningEntries.Any())
        {
            var warningViewModel = new TrainingExpiringSoonWarningViewModel
            {
                Preheader = "",
                SenderName = "",
                SenderTitle = "",
                Title = "[Aurora College] Mandatory Training Expiry"
            };

            foreach (var entry in warningEntries)
            {
                warningViewModel.Courses.Add(entry.Key, entry.Value.ToShortDateString());
            }

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiringSoonWarningEmail.cshtml", warningViewModel);

            var toRecipients = new Dictionary<string, string>();

            if (!toRecipients.Any(recipient => recipient.Value == notification.Teacher.Email))
                toRecipients.Add(notification.Teacher.Name, notification.Teacher.Email);

            await _emailSender.Send(toRecipients, null, warningViewModel.Title, body);
        }
    }
}
