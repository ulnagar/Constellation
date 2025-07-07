namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialTeacherAssignment;

using Core.Models.StaffMembers.Identifiers;

public class TutorialTeacherAssignmentSelection
{
    public StaffId StaffId { get; set; }
    public bool LimitedTime { get; set; }
    public DateTime EffectiveTo { get; set; } = DateTime.Today;

    public Dictionary<StaffId, string> StaffList { get; set; }
}
