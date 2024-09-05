namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialStudentEnrolment;

using Core.Models.Students.Identifiers;

public class TutorialStudentEnrolmentSelection
{
    public StudentId StudentId { get; set; }
    public bool LimitedTime { get; set; }
    public DateTime EffectiveTo { get; set; } = DateTime.Today;

    public Dictionary<StudentId, string> StudentList { get; set; }
}
