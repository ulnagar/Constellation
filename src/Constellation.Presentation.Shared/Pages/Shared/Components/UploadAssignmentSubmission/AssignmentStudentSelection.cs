namespace Constellation.Presentation.Shared.Pages.Shared.Components.UploadAssignmentSubmission;

using Microsoft.AspNetCore.Http;

public class AssignmentStudentSelection
{
    public string StudentId { get; set; }
    public IFormFile? File { get; set; }

    public Dictionary<string, string> StudentList { get; set; }
}
