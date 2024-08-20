namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveTeacherFromTutorialModal;

using Core.Models.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;

public sealed class RemoveTeacherFromTutorialModalViewModel
{
    [ModelBinder(typeof(ConstructorBinder))]
    public TutorialTeacherId Id { get; set; }
    public string Name { get; set; }
    public bool Immediate { get; set; } = true;
    public DateTime EffectiveOn { get; set; } = DateTime.Today;
}
