namespace Constellation.Presentation.Server.Pages.Shared.Components.FamilyAddStudent;

using Constellation.Core.Models.Identifiers;

public class FamilyAddStudentSelection
{
    public FamilyId FamilyId { get; set; }
    public string FamilyName { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;
    public Dictionary<string, string> Students { get; set; } = new();
}
