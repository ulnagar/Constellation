namespace Constellation.Presentation.Server.Pages.Shared.Components.UploadAssignmentSubmission;

public class AssignmentStudentSelection
{
    public string StudentId { get; set; }
    public IFormFile File { get; set; }

    public Dictionary<string, string> StudentList { get; set; }
}
