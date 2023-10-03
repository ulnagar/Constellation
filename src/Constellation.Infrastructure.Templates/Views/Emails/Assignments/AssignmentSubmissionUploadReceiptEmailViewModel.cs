namespace Constellation.Infrastructure.Templates.Views.Emails.Assignments;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class AssignmentSubmissionUploadReceiptEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string CourseName { get; set; }
    public string AssignmentName { get; set; }
    public DateOnly SubmittedOn { get; set; }
}
