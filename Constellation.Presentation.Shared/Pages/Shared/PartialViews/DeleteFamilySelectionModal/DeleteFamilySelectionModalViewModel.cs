namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.DeleteFamilySelectionModal;

using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;

public sealed class DeleteFamilySelectionModalViewModel
{
    public string FamilyName { get; set; } = string.Empty;
    public FamilyId FamilyId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public ParentId ParentId { get; set; }
    public List<string> OtherParentNames { get; set; } = new();
}
