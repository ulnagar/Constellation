namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.DeleteRoleModal;
public sealed record DeleteRoleModalViewModel(
    Guid ContactId,
    Guid RoleId,
    string ContactName,
    string ContactRole,
    string SchoolName);
