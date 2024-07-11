namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.DeleteFamilyConfirmationModal;

using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;

public sealed class DeleteFamilyConfirmationModalViewModel
{
    public FamilyId FamilyId { get; set; }
    public List<string> OtherParentNames { get; set; } = new();

}
