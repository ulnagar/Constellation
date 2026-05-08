namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddSubmissionToAssessment;

using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Http;

public class AssessmentSubmissionSelection
{
    public StudentId StudentId { get; set; } = StudentId.Empty;
    public IFormFile? File { get; set; }

    public required Dictionary<StudentId, string> StudentList { get; set; }
}
