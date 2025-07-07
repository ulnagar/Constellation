namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddSentralIncidentStatusAction;

using Core.Models.StaffMembers.Identifiers;

public sealed class AddSentralIncidentStatusActionViewModel
{
    public StaffId StaffId { get; set; }

    public Dictionary<StaffId, string> StaffMembers { get; set; }
}
