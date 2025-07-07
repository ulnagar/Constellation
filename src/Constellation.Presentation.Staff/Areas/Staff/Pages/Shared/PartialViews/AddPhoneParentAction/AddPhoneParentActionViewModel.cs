namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddPhoneParentAction;

using Core.Models.StaffMembers.Identifiers;

public sealed class AddPhoneParentActionViewModel
{
    public StaffId StaffId { get; set; }

    public Dictionary<StaffId, string> StaffMembers { get; set; }
}
