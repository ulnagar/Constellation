namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Assessments;
using Core.Shared;
using Core.ValueObjects;
using System.Threading.Tasks;
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
            SubmittedOn = submission.SubmittedAt.ToLocalTime().LocalDateTime,
            SubmissionId = submission.Id
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

    public async Task<Result> SendAssessmentNotificationToSchools(
        Assessment assessment,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        AssessmentNotificationForSchoolsEmailViewModel viewModel = new()
        {
            Title = "Assessment Notification",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            AssessmentName = assessment.Name,
            CourseName = assessment.Course,
            DueDate = DateOnly.FromDateTime(assessment.DueDate.LocalDateTime)
        };

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.AuroraCollege,
            "Assessments",
            viewModel.Title,
            recipients,
            cancellationToken: cancellationToken);
    }
}
