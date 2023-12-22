namespace Constellation.Presentation.Server.Pages.Shared.Components.ClassListNavBar;

using Core.Models.Offerings.Identifiers;

public class ClassListNavBarViewModel
{
    public Dictionary<string, OfferingId> Classes { get; set; } = new();
}
