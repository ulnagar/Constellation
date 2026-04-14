namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Application.Domains.Compliance.Assessments.Models;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Assessments;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Templates.Views.Emails.AssessmentProvisions;
using Templates.Views.Emails.Assessments;

public sealed partial class Service : IEmailService
{

    public async Task<Result> SendAssessmentSubmissionReceipt(
        Assessment assessment,
        AssessmentStudent student,
        AssessmentSubmission submission,
        CancellationToken cancellationToken = default)
    {
        AssessmentSubmissionReceiptEmailViewModel viewModel = new()
        {
            Title = $"Assessment Submission Received",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            StudentName = student.Student,
            CourseName = assessment.Course,
            AssignmentName = assessment.Name,
            SubmittedOn = submission.SubmittedAt.ToLocalTime().LocalDateTime
        };

        Result<EmailRecipient> recipient = EmailRecipient.Create(submission.SubmittedBy, submission.SubmittedByEmail);

        if (recipient.IsFailure)
        {
            return recipient;
        }

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Assessments",
            viewModel.Title,
            [ recipient.Value ],
            cancellationToken: cancellationToken);
    }

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

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Assessment Provisions",
            viewModel.Title,
            recipients,
            ccRecipients: ccRecipients,
            cancellationToken: cancellationToken);
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

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Assessment Provisions",
            viewModel.Title,
            recipients,
            ccRecipients: ccRecipients,
            cancellationToken: cancellationToken);
    }
}
