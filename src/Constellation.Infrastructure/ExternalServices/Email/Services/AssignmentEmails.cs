namespace Constellation.Infrastructure.ExternalServices.Email.Services;

using Constellation.Application.Interfaces.Services;
using Core.Models.Assignments;
using Core.Models.Assignments.Identifiers;
using Core.Models.SchoolContacts;
using Core.Models.Students;
using Core.Models.Subjects;
using Core.Shared;
using Core.ValueObjects;
using System;
using System.Threading.Tasks;
using Templates.Views.Emails.Assignments;

public sealed partial class Service : IEmailService
{
    public async Task<Result> SendAssignmentUploadReceipt(
        CanvasAssignment assignment,
        CanvasAssignmentSubmission submission,
        Course course,
        Student student,
        SchoolContact contact,
        CancellationToken cancellationToken = default)
    {
        AssignmentSubmissionUploadReceiptEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Student Assignment Upload Receipt",
            AssignmentName = assignment.Name,
            CourseName = course.Name,
            StudentName = student.Name.DisplayName,
            SubmittedOn = DateOnly.FromDateTime(submission.SubmittedOn)
        };

        Result<EmailRecipient> recipient = contact.GetEmailRecipient();

        if (recipient.IsFailure)
        {
            _logger
                .ForContext(nameof(AssignmentSubmissionUploadReceiptEmailViewModel), viewModel, true)
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to send Assignment Upload Receipt");
            
            return Result.Failure(recipient.Error);
        }

        return await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            "Assignments",
            viewModel.Title,
            [recipient.Value],
            cancellationToken: cancellationToken);
    }

    public async Task SendAssignmentUploadFailedNotification(
        string assignmentName,
        AssignmentId assignmentId,
        string studentName,
        AssignmentSubmissionId submissionId,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $@"<p>Failed to upload an assignment submission to Canvas:</p>
            <dl>
                <dt>Assignment:</dt>
                <dd>{assignmentName} ({assignmentId.Value})</dd>
                <dt>Submission:</dt>
                <dd>From {studentName} ({submissionId.Value})</dd>
            </dl>";

        await BuildAndSendEmail(
            viewModel,
            EmailRecipient.NoReply,
            $"[Aurora College] Canvas Assignment Upload Failure: {assignmentName}",
            [EmailRecipient.InfoTechTeam],
            cancellationToken: cancellationToken);
    }
}
