namespace Constellation.Core.Models.Assessments;

using Auth;
using Core.ValueObjects;
using Identifiers;
using Shared;

public sealed class AssessmentSubmission
{
    public AssessmentSubmission(
        AssessmentStudentId assessmentStudentId,
        AppUser user)
    {
        Id = new();

        AssessmentStudentId = assessmentStudentId;
        SubmittedAt = DateTimeOffset.UtcNow;
        SubmittedBy = user.Name;

        Result<EmailAddress> email = EmailAddress.Create(user.Email);

        if (email.IsSuccess)
            SubmittedByEmail = email.Value;
        else
            SubmittedByEmail = EmailAddress.None;
    }

    public SubmissionId Id { get; init; }
    public AssessmentStudentId AssessmentStudentId { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; }
    public string SubmittedBy { get; private set; }
    public EmailAddress SubmittedByEmail { get; private set; }
}