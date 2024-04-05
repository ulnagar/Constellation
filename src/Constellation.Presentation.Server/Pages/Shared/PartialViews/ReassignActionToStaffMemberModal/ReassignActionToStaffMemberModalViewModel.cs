namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.ReassignActionToStaffMemberModal;

using Core.Models.WorkFlow.Identifiers;
using System.ComponentModel.DataAnnotations;

public class ReassignActionToStaffMemberModalViewModel
{
    public ActionId ActionId { get; set; }
    public string StaffId { get; set; }
    public Dictionary<string, string> StaffMembers { get; set; } = new();
}
