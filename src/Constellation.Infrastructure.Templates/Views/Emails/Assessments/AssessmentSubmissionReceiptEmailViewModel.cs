namespace Constellation.Infrastructure.Templates.Views.Emails.Assessments;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class AssessmentSubmissionReceiptEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Assessments/AssessmentSubmissionReceiptEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string StudentName { get; init; }
    public required string CourseName { get; init; }
    public required string AssignmentName { get; init; }
    public required DateTime SubmittedOn { get; init; }
}
