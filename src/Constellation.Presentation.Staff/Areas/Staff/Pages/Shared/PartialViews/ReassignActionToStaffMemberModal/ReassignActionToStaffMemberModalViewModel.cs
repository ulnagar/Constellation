namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.ReassignActionToStaffMemberModal;

using Constellation.Core.Models.WorkFlow.Identifiers;
using Core.Models.StaffMembers.Identifiers;

public class ReassignActionToStaffMemberModalViewModel
{
    public ActionId ActionId { get; set; }
    public StaffId StaffId { get; set; }
    public Dictionary<StaffId, string> StaffMembers { get; set; } = new();
}
