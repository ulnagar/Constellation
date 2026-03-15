namespace Constellation.Infrastructure.Templates.Views.Emails.Assignments;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class AssignmentSubmissionUploadReceiptEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Assignments/AssignmentSubmissionUploadReceiptEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string StudentName { get; init; }
    public required string CourseName { get; init; }
    public required string AssignmentName { get; init; }
    public required DateOnly SubmittedOn { get; init; }
}
