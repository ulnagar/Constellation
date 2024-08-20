namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.RemoveStudentFromTutorialModal;

using Core.Models.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;
using System;

public sealed class RemoveStudentFromTutorialModalViewModel
{
    [ModelBinder(typeof(ConstructorBinder))]
    public TutorialEnrolmentId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Immediate { get; set; } = true;
    public DateTime EffectiveOn { get; set; } = DateTime.Today;
}
