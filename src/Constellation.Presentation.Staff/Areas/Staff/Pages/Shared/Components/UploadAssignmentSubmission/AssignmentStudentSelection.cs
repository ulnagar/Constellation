namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UploadAssignmentSubmission;

using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Http;

public class AssignmentStudentSelection
{
    public StudentId StudentId { get; set; } = StudentId.Empty;
    public IFormFile? File { get; set; }

    public Dictionary<StudentId, string> StudentList { get; set; }
}
