namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ClassListNavBar;

using Constellation.Core.Models.Offerings.Identifiers;

public class ClassListNavBarViewModel
{
    public Dictionary<string, OfferingId> Classes { get; set; } = new();
}
