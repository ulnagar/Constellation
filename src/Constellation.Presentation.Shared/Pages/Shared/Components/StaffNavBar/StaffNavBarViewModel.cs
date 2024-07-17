namespace Constellation.Presentation.Shared.Pages.Shared.Components.StaffNavBar;

using Core.Models.Offerings.Identifiers;

public sealed class StaffNavBarViewModel
{
    public Dictionary<string, OfferingId> Classes { get; set; } = new();

    public bool CanAccessParentPortal { get; set; }
    public bool CanAccessSchoolPortal { get; set; }
}