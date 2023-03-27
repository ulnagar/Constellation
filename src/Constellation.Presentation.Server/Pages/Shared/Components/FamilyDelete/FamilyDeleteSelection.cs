namespace Constellation.Presentation.Server.Pages.Shared.Components.FamilyDelete;

using Constellation.Core.Models.Identifiers;

public class FamilyDeleteSelection
{
    public string FamilyName { get; set; } = string.Empty;
    public FamilyId FamilyId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public ParentId ParentId { get; set; }
    public List<string> OtherParentNames { get; set; } = new();
}
