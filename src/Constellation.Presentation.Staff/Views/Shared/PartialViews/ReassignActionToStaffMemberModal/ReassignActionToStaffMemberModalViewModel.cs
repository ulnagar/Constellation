namespace Constellation.Presentation.Staff.Views.Shared.PartialViews.ReassignActionToStaffMemberModal;

using Constellation.Core.Models.WorkFlow.Identifiers;

public class ReassignActionToStaffMemberModalViewModel
{
    public ActionId ActionId { get; set; }
    public string StaffId { get; set; }
    public Dictionary<string, string> StaffMembers { get; set; } = new();
}
