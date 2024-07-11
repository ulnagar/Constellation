namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.DeleteFamilyMemberConfirmationModal;

using Constellation.Core.Models.Identifiers;

public sealed class DeleteFamilyMemberConfirmationModalViewModel
{
    public string Title { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public ParentId ParentId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public FamilyId FamilyId { get; set; }
}
