namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.Compliance.Assessments.Models;
using Constellation.Application.Interfaces.Services;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Templates.Views.Emails.AssessmentProvisions;

public sealed partial class Service : IEmailService
{
    public async Task<Result> SendAssessmentProvisionEmailToFamilies(
        List<EmailRecipient> recipients,
        List<EmailRecipient> ccRecipients,
        StudentProvisions provisions,
        CancellationToken cancellationToken = default)
    {
        AssessmentProvisionNotificationForFamiliesEmailViewModel viewModel = new()
        {
            Title = $"Upcoming Examinations – School-Based Adjustments",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            Student = provisions
        };

        string body = await _razorService.RenderViewToStringAsync(AssessmentProvisionNotificationForFamiliesEmailViewModel.ViewLocation, viewModel);

        return await _emailSender.Send(recipients, ccRecipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);
    }

    public async Task<Result> SendAssessmentProvisionEmailToSchools(
        List<EmailRecipient> recipients,
        List<EmailRecipient> ccRecipients,
        Name contact,
        List<StudentProvisions> students,
        CancellationToken cancellationToken = default)
    {
        AssessmentProvisionNotificationForSchoolsEmailViewModel viewModel = new()
        {
            Title = $"Upcoming Examinations – School-Based Adjustments for Your Students",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            Contact = contact,
            Students = students
        };

        string body = await _razorService.RenderViewToStringAsync(AssessmentProvisionNotificationForSchoolsEmailViewModel.ViewLocation, viewModel);

        return await _emailSender.Send(recipients, ccRecipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);
    }
}
