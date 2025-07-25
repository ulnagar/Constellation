namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddTeamToTutorial;

using Core.Models.Tutorials.Identifiers;

public class AddTeamToTutorialSelection
{
    public TutorialId TutorialId { get; set; }
    public Guid TeamId { get; set; }
    public string TutorialName { get; set; }

    public Dictionary<Guid, string> Teams { get; set; } = [];
}
