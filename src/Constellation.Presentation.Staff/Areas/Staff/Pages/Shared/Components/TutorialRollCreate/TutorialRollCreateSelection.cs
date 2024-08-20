namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialRollCreate;

public sealed class TutorialRollCreateSelection
{
    public Guid TutorialId { get; set; }
    public DateTime RollDate { get; set; } = DateTime.Today;
}
