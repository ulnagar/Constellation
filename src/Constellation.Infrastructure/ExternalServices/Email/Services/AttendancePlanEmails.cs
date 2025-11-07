namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Attendance;
using Constellation.Infrastructure.Templates.Views.Emails.AttendancePlans;
using Core.Extensions;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed partial class Service : IEmailService
{
    public async Task SendAttendancePlanToAdmin(
    List<EmailRecipient> recipients,
    AttendancePlan plan,
    CancellationToken cancellationToken = default)
    {
        AttendancePlanDetailsOfUnavailabilityEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Attendance Plan Details",
            Student = plan.Student.DisplayName,
            Grade = plan.Grade.AsName(),
            School = plan.School
        };

        List<AttendancePlanDetailsOfUnavailabilityEmailViewModel.Unavailability> unavailabilities = new();

        foreach (var period in plan.Periods)
        {
            if (period.EntryTime != period.StartTime)
            {
                unavailabilities.Add(new()
                {
                    Week = period.Week,
                    Day = period.Day,
                    Start = period.StartTime,
                    End = period.EntryTime
                });
            }

            if (period.ExitTime != period.EndTime)
            {
                unavailabilities.Add(new()
                {
                    Week = period.Week,
                    Day = period.Day,
                    Start = period.ExitTime,
                    End = period.EndTime
                });
            }
        }

        viewModel.Unavailabilities = unavailabilities;

        string body = await _razorService.RenderViewToStringAsync(AttendancePlanDetailsOfUnavailabilityEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(
            toRecipients: recipients,
            fromRecipient: EmailRecipient.AuroraCollege,
            subject: viewModel.Title,
            body: body,
        cancellationToken: cancellationToken);
    }

    public async Task SendAttendancePlanRejectedNotificationToSchool(
        List<EmailRecipient> recipients,
        AttendancePlan plan,
        string comment,
        CancellationToken cancellationToken = default)
    {
        AttendancePlanRejectedNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Attendance Plan Rejected",
            Student = plan.Student.DisplayName,
            Grade = plan.Grade.AsName(),
            Comment = comment
        };

        string body = await _razorService.RenderViewToStringAsync(AttendancePlanRejectedNotificationEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(
            toRecipients: recipients,
            fromRecipient: EmailRecipient.AuroraCollege,
            subject: viewModel.Title,
            body: body,
            cancellationToken: cancellationToken);
    }
}
