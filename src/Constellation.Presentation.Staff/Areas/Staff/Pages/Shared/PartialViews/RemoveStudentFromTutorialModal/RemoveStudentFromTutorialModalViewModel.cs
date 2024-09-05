namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveStudentFromTutorialModal;

using Core.Models.Identifiers;
using System;

public sealed class RemoveStudentFromTutorialModalViewModel
{
    public TutorialEnrolmentId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Immediate { get; set; } = true;
    public DateTime EffectiveOn { get; set; } = DateTime.Today;
}
