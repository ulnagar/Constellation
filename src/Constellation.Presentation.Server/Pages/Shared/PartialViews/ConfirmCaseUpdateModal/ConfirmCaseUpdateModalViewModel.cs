namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.ConfirmCaseUpdateModal;

using Core.Models.WorkFlow.Identifiers;

public sealed record ConfirmCaseUpdateModalViewModel(
    Guid CaseId,
    ConfirmCaseUpdateModalViewModel.UpdateType Type)
{
    public enum UpdateType
    {
        Complete,
        Cancel
    }
}
