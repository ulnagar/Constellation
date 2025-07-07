namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddSendEmailAction;

using Core.Models.StaffMembers.Identifiers;

public sealed class AddSendEmailActionViewModel
{
    public StaffId StaffId { get; set; }

    public Dictionary<StaffId, string> StaffMembers { get; set; }
}
