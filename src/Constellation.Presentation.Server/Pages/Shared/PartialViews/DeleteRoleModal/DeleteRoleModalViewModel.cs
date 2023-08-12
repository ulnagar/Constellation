namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.DeleteRoleModal;

internal sealed record DeleteRoleModalViewModel(
    int RoleId,
    string ContactName,
    string ContactRole,
    string SchoolName);
