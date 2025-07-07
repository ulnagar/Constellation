namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddParentInterviewAction;

using Core.Models.StaffMembers.Identifiers;

public sealed class AddParentInterviewActionViewModel
{
    public StaffId StaffId { get; set; }

    public Dictionary<StaffId, string> StaffMembers { get; set; }
}
