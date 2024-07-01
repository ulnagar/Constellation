namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialStudentEnrolment;

public class TutorialStudentEnrolmentSelection
{
    public string StudentId { get; set; }
    public bool LimitedTime { get; set; }
    public DateTime EffectiveTo { get; set; } = DateTime.Today;

    public Dictionary<string, string> StudentList { get; set; }
}
