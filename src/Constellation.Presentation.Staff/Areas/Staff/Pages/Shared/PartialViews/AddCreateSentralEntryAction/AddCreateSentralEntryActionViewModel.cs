namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddCreateSentralEntryAction;

using Core.Models.StaffMembers.Identifiers;

public class AddCreateSentralEntryActionViewModel
{
    public Guid OfferingId { get; set; }
    public StaffId StaffId { get; set; } = StaffId.Empty;

    public Dictionary<StaffId, string> StaffMembers { get; set; }
    public Dictionary<Guid, string> Offerings { get; set; }
}
