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
}
