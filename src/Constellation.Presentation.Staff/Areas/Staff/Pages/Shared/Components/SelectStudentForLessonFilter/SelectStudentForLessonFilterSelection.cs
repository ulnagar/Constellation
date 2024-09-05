namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.SelectStudentForLessonFilter;

using Core.Models.Students.Identifiers;

public class SelectStudentForLessonFilterSelection
{
    public StudentId StudentId { get; set; } = StudentId.Empty;
    public Dictionary<StudentId, string> StudentList { get; set; }
}
