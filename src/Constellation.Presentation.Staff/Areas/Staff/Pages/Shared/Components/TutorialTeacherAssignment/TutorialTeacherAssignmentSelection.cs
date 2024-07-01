namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialTeacherAssignment;

public class TutorialTeacherAssignmentSelection
{
    public string StaffId { get; set; }
    public bool LimitedTime { get; set; }
    public DateTime EffectiveTo { get; set; } = DateTime.Today;

    public Dictionary<string, string> StaffList { get; set; }
}
