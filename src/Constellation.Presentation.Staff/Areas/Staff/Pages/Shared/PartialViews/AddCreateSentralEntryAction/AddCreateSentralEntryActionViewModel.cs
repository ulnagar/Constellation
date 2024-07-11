namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddCreateSentralEntryAction;

public class AddCreateSentralEntryActionViewModel
{
    public Guid OfferingId { get; set; }
    public string StaffId { get; set; }

    public Dictionary<string, string> StaffMembers { get; set; }
    public Dictionary<Guid, string> Offerings { get; set; }
}
