namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialRollAddStudent;

using Core.Models.Students.Identifiers;

public class TutorialRollAddStudentSelection
{
    public StudentId StudentId { get; set; }
    
    public Dictionary<StudentId, string> StudentList { get; set; }
}
