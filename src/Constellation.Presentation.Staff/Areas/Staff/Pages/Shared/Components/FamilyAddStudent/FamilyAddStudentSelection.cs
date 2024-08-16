namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.FamilyAddStudent;

using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;

public class FamilyAddStudentSelection
{
    [ModelBinder(typeof(ConstructorBinder))]
    public FamilyId FamilyId { get; set; }
    public string FamilyName { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;
    public Dictionary<string, string> Students { get; set; } = new();
}
