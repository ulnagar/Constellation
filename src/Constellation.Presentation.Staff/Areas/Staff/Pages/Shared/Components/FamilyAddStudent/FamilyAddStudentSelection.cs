namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.FamilyAddStudent;

using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;

public class FamilyAddStudentSelection
{
    public FamilyId FamilyId { get; set; }
    public string FamilyName { get; set; } = string.Empty;

    public StudentId StudentId { get; set; } = StudentId.Empty;
    public Dictionary<StudentId, string> Students { get; set; } = new();
}
