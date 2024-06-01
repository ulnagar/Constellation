namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.DeleteFamilyMemberConfirmationModal;

public sealed class DeleteFamilyMemberConfirmationModalViewModel
{
    public string Title { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public Guid ParentId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
}
