namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.EnrolStudentInTutorial;

using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Tutorials.Identifiers;

public class EnrolStudentInTutorialSelection
{
    public TutorialId TutorialId { get; set; }
    public string TutorialName { get; set; } = string.Empty;

    public StudentId StudentId { get; set; } = StudentId.Empty;
    public Dictionary<StudentId, string> Students { get; set; } = new();
}
