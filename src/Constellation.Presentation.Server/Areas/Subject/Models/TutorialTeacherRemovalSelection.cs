namespace Constellation.Presentation.Server.Areas.Subject.Models;

public sealed class TutorialTeacherRemovalSelection
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Immediate { get; set; } = true;
    public DateTime EffectiveOn { get; set; } = DateTime.Today;
}
