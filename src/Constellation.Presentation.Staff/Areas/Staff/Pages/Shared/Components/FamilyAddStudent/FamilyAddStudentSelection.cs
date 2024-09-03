namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.FamilyAddStudent;

using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Mvc;

public class FamilyAddStudentSelection
{
    [ModelBinder(typeof(ConstructorBinder))]
    public FamilyId FamilyId { get; set; }
    public string FamilyName { get; set; } = string.Empty;

    [ModelBinder(typeof(ConstructorBinder))]
    public StudentId StudentId { get; set; }
    public Dictionary<string, string> Students { get; set; } = new();
}
