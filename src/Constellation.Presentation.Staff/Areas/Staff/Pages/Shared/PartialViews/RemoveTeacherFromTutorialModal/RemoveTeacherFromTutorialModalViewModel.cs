namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveTeacherFromTutorialModal;

using Core.Models.Identifiers;

public sealed class RemoveTeacherFromTutorialModalViewModel
{
    public TutorialTeacherId Id { get; set; }
    public string Name { get; set; }
    public bool Immediate { get; set; } = true;
    public DateTime EffectiveOn { get; set; } = DateTime.Today;
}
