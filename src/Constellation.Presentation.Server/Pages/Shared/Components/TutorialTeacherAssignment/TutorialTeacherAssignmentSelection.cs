namespace Constellation.Presentation.Server.Pages.Shared.Components.TutorialTeacherAssignment;

public class TutorialTeacherAssignmentSelection
{
    public string StaffId { get; set; }
    public bool LimitedTime { get; set; }
    public DateTime EffectiveTo { get; set; }

    public Dictionary<string, string> StaffList { get; set; }
}
